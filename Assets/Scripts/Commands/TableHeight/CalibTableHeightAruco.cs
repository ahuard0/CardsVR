using CardsVR.Commands;
using CardsVR.Interaction;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using CardsVR.Utility;
using System.Collections.Generic;
using System.Collections;
using CardsVR.Networking;

namespace CardsVR.Detection
{
    public class CalibTableHeightAruco : MonoBehaviour, IObserver
    {
        [Header("Camera Manager")]
        [SerializeField]
        private CameraManager cameraManager;

        [Header("Parameters")]
        [SerializeField]
        private int[] SearchArucoIDs;
        [SerializeField]
        private float s = 0.0195f;
        [SerializeField]
        private float initialHeight = 0.8024f;
        [SerializeField]
        private int numRequiredFrames = 10;

        [Header("Game Objects")]
        [SerializeField]
        private GameObject Tabletop;
        [SerializeField]
        private GameObject Table;
        [SerializeField]
        private GameObject[] Chairs;

        [Header("Data")]
        public float PlayerTableHeightWorld;
        public bool clearTagCount = false;
        public int countFrames = 0;
        public List<Vector3> TagPoints3D = new List<Vector3>();

        private void OnEnable()
        {
            ArucoClient.Instance.AttachObserver(this);
            StartCoroutine("CalibrateWhenReady");
        }

        private void OnDisable()
        {
            ArucoClient.Instance.DetachObserver(this);
            StopCoroutine("CalibrateWhenReady");
        }

        private void Update()
        {
            if (clearTagCount)
            {
                clearTagCount = false;  // Disable further calibration
                resetCount();
            }
        }

        private IEnumerator CalibrateWhenReady()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(0.5f);

                if (PlayerManager.Instance.PlayerNum == 0)  // wait for the player ID number to be assigned.
                    continue;

                if (!isReadyToCalibrate())  // only calibrate in the beginning of the game and disable tag 3D projection when no longer needed to calibrate (a computationally expensive process)
                    continue;

                calibrateTableHeight();
                resetCount();
            }
        }

        private void resetCount()
        {
            countFrames = 0;
            TagPoints3D.Clear();
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
                if (countFrames >= numRequiredFrames && TagPoints3D.Count > 0)
                    return true;
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
         *      Searches for the calibration board Aruco Tag IDs and returns a list of tag coordinates.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      return : List<Vector3>
         *          
         */
        private List<Vector3> getTagPoints3D()
        {
            Camera cam = cameraManager.ExternalCamera;  // The iPhone Camera
            if (cam == null)
                return null;

            FrameData frameData = ArucoClient.Instance.Data;  // Frame Data includes Camera Intrinsics
            ArucoData tagData = frameData.ArucoData;  // The Aruco Tag data within the Frame

            int frameHeight = frameData.ArucoData.height;  // e.g., 720
            int frameWidth = frameData.ArucoData.width;  // e.g., 1280

            cam.aspect = (float)frameWidth / (float)frameHeight;  // e.g., 1.78
            cam.pixelRect = new UnityEngine.Rect(0, 0, frameWidth, frameHeight);  // e.g., [0 0 1280 720]
            cam.gateFit = Camera.GateFitMode.Vertical;

            Matrix4x4 M_CamIntrinsic = getIntrinsicCameraMatrix(frameData);  // Intrinsic Camera Projection:  3D Camera Space to 2D Screen Space.  Assumes camera position and rotation are zero and at the origin (no extrinsic information).  Extrinsic translation and rotation will be performed later based on Oculus sensor data.
            Mat intrinsicCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
            for (int i = 0; i < intrinsicCamMatrix.rows(); i++)
                for (int j = 0; j < intrinsicCamMatrix.cols(); j++)
                    intrinsicCamMatrix.put(i, j, M_CamIntrinsic[i, j]);

            MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);  // Distortion Coefficients are Zero for Simple Camera.  iPhone distortion coefficients can be set to zero.

            List<Vector3> TagPoints3D = new List<Vector3>();
            foreach (Tag tag in tagData.Tags)
            {
                int tagID = tag.getUniqueID();
                if (!Toolbox.Exists<int>(SearchArucoIDs, tagID))  // tag ID must be in search group
                    continue;

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
                    return null;
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


                // Get Camera Position and Rotation
                Vector3 CamPosition = cam.transform.position;
                Quaternion CamRotation = cam.transform.rotation;

                Matrix4x4 M_CamExtrinsic = Matrix4x4.TRS(CamPosition, CamRotation, Vector3.one);  // Camera Extrinsic: position, rotation, scale
                Matrix4x4 M_ModelToCamera = M_CamExtrinsic * M_ModelToWorld;

                // Compute Output : World Coords
                Vector3[] P_Camera = new Vector3[4];
                P_Camera[0] = M_ModelToCamera.MultiplyPoint3x4(modelPts[0]);
                P_Camera[1] = M_ModelToCamera.MultiplyPoint3x4(modelPts[1]);
                P_Camera[2] = M_ModelToCamera.MultiplyPoint3x4(modelPts[2]);
                P_Camera[3] = M_ModelToCamera.MultiplyPoint3x4(modelPts[3]);

                Rvec.Dispose();
                Tvec.Dispose();
                Rvec_aux.Dispose();
                Tvec_aux.Dispose();
                rotMat.Dispose();

                TagPoints3D.AddRange(new List<Vector3>(P_Camera));
            }

            return TagPoints3D;
        }

        /*
         *      Sets the height of the table and chairs in world coordinates.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void setTableHeightWorld(float heightWorld)
        {
            PlayerTableHeightWorld = heightWorld;

            // Scale Table
            float Scale_Table = PlayerTableHeightWorld / initialHeight;
            Vector3 scale = Table.transform.localScale;
            scale.y = Scale_Table;
            Table.transform.localScale = scale;

            // Scale Tabletop
            Vector3 position = Tabletop.transform.position;
            position.y = heightWorld;
            Tabletop.transform.position = position;

            // Scale Chairs
            foreach (GameObject chair in Chairs)
            {
                scale = chair.transform.localScale;
                scale.y = Scale_Table;
                chair.transform.localScale = scale;
            }
        }

        /*
         *      Calculates the Average Tag Height and Calibrates the Table Height accordingly.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void calibrateTableHeight()
        {
            // Calculate Average Tag Height over all frames captured
            float sum_y = 0;
            foreach (Vector3 TagPoint in TagPoints3D)
                sum_y += TagPoint.y;
            float avg_y = sum_y / TagPoints3D.Count;

            setTableHeightWorld(avg_y);  // Set the table height to the average of the detected tags
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
            if (isGameStart())  // only calibrate in the beginning of the game and disable tag 3D projection when no longer needed to calibrate (a computationally expensive process)
            {
                List<Vector3> Points3D = getTagPoints3D();  // Expensive operation!!!
                if (Points3D.Count >= SearchArucoIDs.Length / 2)
                {
                    TagPoints3D.AddRange(Points3D);
                    countFrames++;
                }
            }
        }
    }
}
