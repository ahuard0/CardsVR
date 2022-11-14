using CardsVR.Commands;
using CardsVR.Interaction;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;
using CardsVR.Networking;
using System.Collections;
using Photon.Pun;

namespace CardsVR.Detection
{
    public class CalibDeckPosArucoClient : MonoBehaviour, IObserver, IOnEventCallback
    {
        [Header("Camera Manager")]
        [SerializeField]
        private CameraManager cameraManager;

        [Header("Game Objects")]
        [SerializeField]
        private GameObject tableTop;
        [SerializeField]
        private GameObject deckPilePlayer1;
        [SerializeField]
        private GameObject deckPilePlayer2;

        [Header("Parameters")]
        [SerializeField]
        private float persistance = 1f;
        [SerializeField]
        private float DepthScalingFactor = 1f;
        [SerializeField]
        private int minFramesCalcCardCentroid = 10;
        [SerializeField]
        private bool drawCentroid = true;
        [SerializeField]
        private float largeTagWeight = 1;

        [Header("Data")]
        [SerializeField]
        private Vector3 CardCentroidTableTop = Vector3.zero;
        [SerializeField]
        private int framesProcessed = 0;
        [SerializeField]
        private List<float> CardTagAreas = new List<float>();
        [SerializeField]
        private List<int> CardUniqueIDs = new List<int>();
        [SerializeField]
        private List<int> CardTagUniqueIDs = new List<int>();
        [SerializeField]
        private List<float> CardTagCentroidsX = new List<float>();
        [SerializeField]
        private List<float> CardTagCentroidsZ = new List<float>();
        [SerializeField]
        private List<float> CardTagCentroidWeights = new List<float>();

        private GameObject PlayerDeckPile
        {
            get
            {
                int playerID = PlayerManager.Instance.PlayerNum;
                if (playerID == 1)
                    return deckPilePlayer1;
                else if (playerID == 2)
                    return deckPilePlayer2;
                else
                    throw new System.Exception("Unknown Player ID.");
            }
        }
        
        private int PlayerDeckPileID
        {
            get
            {
                int playerID = PlayerManager.Instance.PlayerNum;
                if (playerID == 1)
                    return 0;
                else if (playerID == 2)
                    return 7;
                else
                    throw new System.Exception("Unknown Player ID.");
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            ArucoClient.Instance.AttachObserver(this);
            StartCoroutine("CalibrateWhenReady");
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            ArucoClient.Instance.DetachObserver(this);
            StopCoroutine("CalibrateWhenReady");
        }

        private void Update()
        {
            if (drawCentroid && CardCentroidTableTop != Vector3.zero)  // Draw Calibrated Card Centroid
            {
                CardCentroidTableTop.y = tableTop.transform.position.y;  // Get Height of Table Top (Y-Axis Could Change After Prior Calc of X and Z)

                Vector3 CardCentroidTableTopOffset = CardCentroidTableTop;
                CardCentroidTableTopOffset.y += 0.1f;

                Debug.DrawLine(CardCentroidTableTop, CardCentroidTableTopOffset, Color.cyan, persistance);
            }
        }


        private IEnumerator CalibrateWhenReady()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(0.1f);

                if (PlayerManager.Instance.PlayerNum == 0)  // wait for the player ID number to be assigned.
                    continue;

                if (!isReadyToCalibrate())  // only calibrate in the beginning of the game and disable tag 3D projection when no longer needed to calibrate (a computationally expensive process)
                    continue;

                CalibrateCardCentroid();  // Perform Card Position Calibration Onto Table Top Surface
                resetCount();
            }
        }

        private void resetCount()
        {
            framesProcessed = 0;
            CardTagCentroidsX.Clear();
            CardTagCentroidsZ.Clear();
            CardTagUniqueIDs.Clear();
            CardTagCentroidWeights.Clear();
            CardTagAreas.Clear();
        }

        /*
         *      Trigger condition for table height calibration
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      return : bool
         *          
         */
        private bool isReadyToCalibrate()
        {
            if (isGameStart())
                if (framesProcessed >= minFramesCalcCardCentroid)
                    if (CardTagCentroidsX.Count > 0 && CardTagCentroidsZ.Count > 0)
                        return true;
                    else
                        return false;
                else
                    return false;
            else
                return false;
        }

        /*
         *      Game starting conditions where Score 1 and 2 are zero.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      return : bool
         *          
         */
        private bool isGameStart()
        {
            if (GameManager.Instance.StateDominantHand != GameManager.DominantHandState.Free)
                return false;

            if (PlayerManager.Instance.PlayerNum == 1 && GameManager.Instance.getNumCards(4) > 0)
                return false;
            else if (PlayerManager.Instance.PlayerNum == 2 && GameManager.Instance.getNumCards(3) > 0)
                return false;

            int score;
            if (PlayerManager.Instance.PlayerNum == 1)
                score = GameManager.Instance.getNumCards(6);
            else if (PlayerManager.Instance.PlayerNum == 2)
                score = GameManager.Instance.getNumCards(1);
            else
                return false;

            if (score == 0)
                return true;
            else
                return false;
        }

        /*
         *      Sync the player's deck pile position across clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void BroadcastDeckPosition(Vector3 Position)
        {
            int PlayerID = PlayerManager.Instance.PlayerNum;
            int PileID = PlayerDeckPileID;

            DeckPosData data = new DeckPosData(PlayerID, PileID, Position);
            SendData command = new SendData(data: data, SendReliable: true, ReceiveLocally: true);
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
        }

        /*
         *      PUN uses this callback method to respond to PUN events. The client must first be 
         *      registered to receive PUN events.
         *      
         *      DeckPosClient receives events and data from players on PUN, including ourself, using
         *      the OnEvent callback.  For example, invoking the a command will trigger
         *      the OnEvent callback for all players regardless of who sent the data in the first place.
         *      
         *      Parameters
         *      ----------
         *      photonEvent : EventData
         *          Contains a byte event code and an object array containing arbitrary data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == DeckPosData.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                DeckPosData msg = DeckPosData.FromObjectArray(data);
                Command command = new SyncDeckPos(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }

        private int CalcMode(int[] arr)  // https://stackoverflow.com/questions/8260555/how-to-find-the-mode-in-array-c
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            foreach (int a in arr)
            {
                if (counts.ContainsKey(a))
                    counts[a] = counts[a] + 1;
                else
                    counts[a] = 1;
            }

            int result = int.MinValue;
            int max = int.MinValue;
            foreach (int key in counts.Keys)
            {
                if (counts[key] > max)
                {
                    max = counts[key];
                    result = key;
                }
            }

            return result;
        }

        /*
         *      Calculates the average (centroid) of the tags comprising each card.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void CalibrateCardCentroid()
        {
            // Generate Mask : Filter out all cards except the most prominant (most tag hits)
            int[] ArrayCardID = CardUniqueIDs.ToArray();
            int ModeCardID = CalcMode(ArrayCardID);  // The most prominant card (most tag hits -- the Mode)
            bool[] Mask = new bool[ArrayCardID.Length];
            int Count = 0;
            for (int i = 0; i < Mask.Length; i++)
                if (ArrayCardID[i] == ModeCardID)
                {
                    Count++;
                    Mask[i] = true;
                }
                else
                    Mask[i] = false;
            
            // Calculate the sum of the weights (for weighted avg)
            float sum_weights = 0;
            float[] ArrayWeights = CardTagCentroidWeights.ToArray();
            for (int i = 0; i < Mask.Length; i++)
                if (Mask[i])
                    sum_weights += ArrayWeights[i];

            // Calculate the percent contribution of each term (for weighted avg)
            float[] pct_weights = new float[CardTagCentroidWeights.Count];
            float[] arr_weights = CardTagCentroidWeights.ToArray();
            for (int i = 0; i < Mask.Length; i++)
                if (Mask[i])
                    pct_weights[i] = arr_weights[i] / sum_weights;

            // Calculate the X-Centroid
            float avg_x = 0;
            float[] arr_centroidX = CardTagCentroidsX.ToArray();
            for (int i = 0; i < Mask.Length; i++)
                if (Mask[i])
                    avg_x += arr_centroidX[i] * pct_weights[i];
            CardCentroidTableTop.x = avg_x;  // Store Avg

            // Calculate the Z-Centroid
            float avg_z = 0;
            float[] arr_centroidZ = CardTagCentroidsZ.ToArray();
            for (int i = 0; i < Mask.Length; i++)
                if (Mask[i])
                    avg_z += arr_centroidZ[i] * pct_weights[i];
            CardCentroidTableTop.z = avg_z;  // Store Avg

            CardCentroidTableTop.y = tableTop.transform.position.y;

            CardTagCentroidsX.Clear();
            CardTagCentroidsZ.Clear();
            CardTagCentroidWeights.Clear();
            CardUniqueIDs.Clear();
            CardTagUniqueIDs.Clear();
            CardTagAreas.Clear();
            framesProcessed = 0;

            BroadcastDeckPosition(CardCentroidTableTop);  // Command All Clients to Change Pile Position
        }

        /*
         *      Returns the intrinsic camera matrix.
         *      
         *      Parameters
         *      ----------
         *      frameData : FrameData
         *          The frame data object, which contains the camera's frame width, height, 
         *          focal length, and centroid offset.
         *      
         *      Returns
         *      -------
         *      return : Matrix4x4
         *          The Intrinsic Camera Matrix.
         */
        private Matrix4x4 getIntrinsicCameraMatrix(FrameData frameData)
        {
            int frameHeight = frameData.ArucoData.height;
            int frameWidth = frameData.ArucoData.width;

            int max_d = (int)Mathf.Max(frameWidth, frameHeight);
            float fx = max_d;
            float fy = max_d;
            float cx = frameWidth / 2.0f;
            float cy = frameHeight / 2.0f;
            Matrix4x4 camMatrix = new Matrix4x4();
            camMatrix[0, 0] = fx;
            camMatrix[0, 1] = 0;
            camMatrix[0, 2] = cx;
            camMatrix[1, 0] = 0;
            camMatrix[1, 1] = fy;
            camMatrix[1, 2] = cy;
            camMatrix[2, 0] = 0;
            camMatrix[2, 1] = 0;
            camMatrix[2, 2] = 1f;
            return camMatrix;
        }

        /*
         *      Callback function for ArucoClient events.  Called whenever the frame is updated.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Notify()
        {
            if (!isGameStart())  // Avoids expensive processing when the game has started and calibration is no longer needed.
                return;

            Camera cam = cameraManager.ExternalCamera;  // The iPhone Camera
            if (cam == null)
                return;

            FrameData frameData = ArucoClient.Instance.Data;  // Frame Data includes Camera Intrinsics
            ArucoData tagData = frameData.ArucoData;  // The Aruco Tag data within the Frame

            if (tagData.Tags.Count > 0)  // Only process when Aruco Tags are detected
            {
                int frameHeight = frameData.ArucoData.height;  // e.g., 720
                int frameWidth = frameData.ArucoData.width;  // e.g., 1280

                cam.aspect = (float)frameWidth / (float)frameHeight;  // e.g., 1.78
                cam.pixelRect = new UnityEngine.Rect(0, 0, frameWidth, frameHeight);  // e.g., [0 0 1280 720]
                cam.gateFit = Camera.GateFitMode.Vertical;

                Matrix4x4 M_CamIntrinsic = getIntrinsicCameraMatrix(frameData);  // Intrinsic Camera Projection:  3D Camera Space to 2D Screen Space.  Assumes camera position and rotation are zero and at the origin (no extrinsic information).
                Mat intrinsicCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
                for (int i = 0; i < intrinsicCamMatrix.rows(); i++)
                    for (int j = 0; j < intrinsicCamMatrix.cols(); j++)
                        intrinsicCamMatrix.put(i, j, M_CamIntrinsic[i, j]);

                MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);  // Distortion Coefficients are Zero for Simple Camera.  iPhone distortion coefficients can be set to zero.

                List<float> BatchCardTagCentroidsX = new List<float>();
                List<float> BatchCardTagCentroidsZ = new List<float>();
                List<float> BatchCardTagCentroidWeights = new List<float>();
                List<int> BatchCardTagUniqueIDs = new List<int>();
                List<int> BatchCardUniqueIDs = new List<int>();
                List<float> BatchCardAreas = new List<float>();
                foreach (Tag tag in tagData.Tags)
                {
                    int tagUniqueID = tag.getUniqueID();
                    int CardUniqueID = tag.info.CardID;

                    float s;
                    float avgWeight;
                    if ((tagUniqueID % 28 == 26 || tagUniqueID % 28 == 27) && tagUniqueID <= 2911)  // Large Card Center Tags
                    {
                        s = 1f / 0.9f * DepthScalingFactor;
                        avgWeight = largeTagWeight;
                    }
                    else if (tagUniqueID <= 2911)  // Small Tags Around the Perimeter of the Card
                    {
                        continue;
                        //s = 0.3f / 0.9f * DepthScalingFactor;
                        //avgWeight = 1f;
                    }
                    else if (tagUniqueID >= 2912 && tagUniqueID <= 2946)  // 35 Tag Table Height Calibration Board
                        continue;  // skip
                    else  // Unknowns
                        continue; // skip

                    MatOfPoint2f sensorPoints = tag.getCorners2();

                    //  Model Coords : Canonical Model of Tag
                    Vector3[] modelPts = new Vector3[4];
                    modelPts[0] = new Vector3(-0.5f * s, 0, -0.5f * s);
                    modelPts[1] = new Vector3(0.5f * s, 0, -0.5f * s);
                    modelPts[2] = new Vector3(0.5f * s, 0, 0.5f * s);
                    modelPts[3] = new Vector3(-0.5f * s, 0, 0.5f * s);

                    Point3[] cornersArray3 = new Point3[4];
                    cornersArray3[0] = new Point3(modelPts[0].x, modelPts[0].y, modelPts[0].z);
                    cornersArray3[1] = new Point3(modelPts[1].x, modelPts[1].y, modelPts[1].z);
                    cornersArray3[2] = new Point3(modelPts[2].x, modelPts[2].y, modelPts[2].z);
                    cornersArray3[3] = new Point3(modelPts[3].x, modelPts[3].y, modelPts[3].z);
                    MatOfPoint3f modelPoints = new MatOfPoint3f(cornersArray3);

                    Vector3 CardCentroidModel = new Vector3(-DepthScalingFactor * (float)tag.info.ModelX, 0, -DepthScalingFactor * (float)tag.info.ModelY);

                    // Solve for 3D perspective using OpenCV
                    Mat Rvec = new Mat();
                    Mat Tvec = new Mat();
                    Mat Rvec_aux = new Mat();
                    Mat Tvec_aux = new Mat();

                    // Pose Estimation -------------------------------------------------
                    // Iterative PnP Solution is needed for stable results.  Other solver types cause instability and return failure on occasion. Iterative is recommended.
                    bool success = Calib3d.solvePnP(objectPoints: modelPoints, imagePoints: sensorPoints, cameraMatrix: intrinsicCamMatrix, distCoeffs: distCoeffs, rvec: Rvec_aux, tvec: Tvec_aux, useExtrinsicGuess: false, flags: Calib3d.SOLVEPNP_IPPE);  // CV_ITERATIVE
                    if (!success)
                    {
                        Debug.Log("Could not solvePnP.  Skipping frame.");
                        return;
                    }

                    Rvec_aux.convertTo(Rvec, CvType.CV_32F);
                    Tvec_aux.convertTo(Tvec, CvType.CV_32F);

                    Mat rotMat = new Mat(3, 3, CvType.CV_64FC1);
                    Calib3d.Rodrigues(Rvec, rotMat);


                    // Compute Transformation Matrix
                    Matrix4x4 M_ModelToWorld = new Matrix4x4();
                    M_ModelToWorld.SetRow(0, new Vector4((float)rotMat.get(0, 0)[0], (float)rotMat.get(0, 1)[0], (float)rotMat.get(0, 2)[0], (float)Tvec.get(0, 0)[0]));
                    M_ModelToWorld.SetRow(1, new Vector4((float)rotMat.get(1, 0)[0], (float)rotMat.get(1, 1)[0], (float)rotMat.get(1, 2)[0], (float)Tvec.get(1, 0)[0]));
                    M_ModelToWorld.SetRow(2, new Vector4((float)rotMat.get(2, 0)[0], (float)rotMat.get(2, 1)[0], (float)rotMat.get(2, 2)[0], (float)Tvec.get(2, 0)[0]));
                    M_ModelToWorld.SetRow(3, new Vector4(0, 0, 0, 1));

                    Rvec.Dispose();
                    Tvec.Dispose();
                    Rvec_aux.Dispose();
                    Tvec_aux.Dispose();
                    rotMat.Dispose();

                    // Get Camera Position and Rotation
                    Vector3 CamPosition = cam.transform.position;
                    Quaternion CamRotation = cam.transform.rotation;

                    Matrix4x4 M_CamExtrinsic = Matrix4x4.TRS(CamPosition, CamRotation, Vector3.one);  // Camera Extrinsic: position, rotation, scale
                    Matrix4x4 M_ModelToCamera = M_CamExtrinsic * M_ModelToWorld;

                    // Compute Tag Output : World Coords
                    Vector3[] P_Camera = new Vector3[4];
                    P_Camera[0] = M_ModelToCamera.MultiplyPoint3x4(modelPts[0]);
                    P_Camera[1] = M_ModelToCamera.MultiplyPoint3x4(modelPts[1]);
                    P_Camera[2] = M_ModelToCamera.MultiplyPoint3x4(modelPts[2]);
                    P_Camera[3] = M_ModelToCamera.MultiplyPoint3x4(modelPts[3]);

                    // Project Tag Points onto the Table Top
                    for (int i = 0; i < P_Camera.Length; i++)
                        P_Camera[i].y = tableTop.transform.position.y;

                    // Compute Area of Tag Projected on TableTop (for noise rejection)
                    float x1 = P_Camera[0].x;
                    float x2 = P_Camera[1].x;
                    float x3 = P_Camera[2].x;
                    float x4 = P_Camera[3].x;
                    float y1 = P_Camera[0].z;
                    float y2 = P_Camera[1].z;
                    float y3 = P_Camera[2].z;
                    float y4 = P_Camera[3].z;
                    float TagArea = 0.5f * (x1 * y2 + x2 * y3 + x3 * y4 + x4 * y1 - x2 * y1 - x3 * y2 - x4 * y3 - x1 * y4);  // Credit: https://en.wikipedia.org/wiki/Shoelace_formula

                    // Compute Centroid of Card based on Tag
                    Vector3 P_CardCentroidTableTop = M_ModelToCamera.MultiplyPoint3x4(CardCentroidModel);
                    P_CardCentroidTableTop.y = tableTop.transform.position.y;  // Project onto table top
                    Vector3 P_TagCentroidTableTop = M_ModelToCamera.MultiplyPoint3x4(Vector3.zero);
                    P_TagCentroidTableTop.y = tableTop.transform.position.y;

                    // Add Tag Card Centroids to List for Processing Once Full
                    BatchCardTagCentroidsX.Add(P_CardCentroidTableTop.x);
                    BatchCardTagCentroidsZ.Add(P_CardCentroidTableTop.z);
                    BatchCardTagCentroidWeights.Add(avgWeight);
                    BatchCardTagUniqueIDs.Add(tagUniqueID);
                    BatchCardUniqueIDs.Add(CardUniqueID);
                    BatchCardAreas.Add(TagArea);
                }

                // Add Tags to Collection for later averaging when a large enough sample has been collected
                if (BatchCardTagCentroidsX.Count > 0 && BatchCardTagCentroidsZ.Count > 0)
                {
                    CardTagCentroidsX.AddRange(BatchCardTagCentroidsX);
                    CardTagCentroidsZ.AddRange(BatchCardTagCentroidsZ);
                    CardTagCentroidWeights.AddRange(BatchCardTagCentroidWeights);
                    CardTagUniqueIDs.AddRange(BatchCardTagUniqueIDs);
                    CardUniqueIDs.AddRange(BatchCardUniqueIDs);
                    CardTagAreas.AddRange(BatchCardAreas);
                    framesProcessed++;
                }
            }
        }
    }

}
