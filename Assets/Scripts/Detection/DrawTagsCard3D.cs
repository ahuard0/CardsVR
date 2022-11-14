using CardsVR.Commands;
using CardsVR.Interaction;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using CardsVR.Utility;
using System.Collections.Generic;

namespace CardsVR.Detection
{
    /*
     *  This test script is limited to imaging of only one single card at a time.  
     *  Multiple cards will confuse the single card centroid averaging algorithm 
     *  and produce erroneous results.  Make sure only one Aruco card is present 
     *  in the camera view at one time.
     */
    public class DrawTagsCard3D : MonoBehaviour, IObserver
    {
        [Header("Camera Manager")]
        [SerializeField]
        private CameraManager cameraManager;

        [Header("Game Objects")]
        [SerializeField]
        private GameObject tableTop;

        [Header("Parameters")]
        [SerializeField]
        private float persistance = 1f;
        [SerializeField]
        private float DepthScalingFactor = 1f;
        [SerializeField]
        private bool drawCardTags3D = true;
        [SerializeField]
        private bool drawCardTagsTableTop = true;
        [SerializeField]
        private bool drawCardTagCentroids = true;
        [SerializeField]
        private int minFramesCalcCardCentroid = 10;

        [Header("Data")]
        [SerializeField]
        private bool isCardPositionCalibrated = false;
        [SerializeField]
        private Vector3 CardCentroidTableTop = Vector3.zero;
        [SerializeField]
        private int framesProcessed = 0;
        [SerializeField]
        private List<float> CardTagCentroidsX = new List<float>();
        [SerializeField]
        private List<float> CardTagCentroidsZ = new List<float>();

        private void OnEnable()
        {
            ArucoClient.Instance.AttachObserver(this);
        }

        private void OnDisable()
        {
            ArucoClient.Instance.DetachObserver(this);
        }

        private void Update()
        {
            if (isCardPositionCalibrated)  // Draw Calibrated Card Centroid
            {
                CardCentroidTableTop.y = tableTop.transform.position.y;  // Get Height of Table Top (Y-Axis Could Change After Prior Calc of X and Z)

                Vector3 CardCentroidTableTopOffset = CardCentroidTableTop;
                CardCentroidTableTopOffset.y += 0.1f;

                Debug.DrawLine(CardCentroidTableTop, CardCentroidTableTopOffset, Color.cyan, persistance);
            } 
            else  
                CalibrateCardCentroid();  // Perform Card Position Calibration Onto Table Top Surface
        }

        private void CalibrateCardCentroid()
        {
            if (framesProcessed >= minFramesCalcCardCentroid)
            {
                if (CardTagCentroidsX.Count > 0 && CardTagCentroidsZ.Count > 0)
                {
                    float sum_x = 0;
                    foreach (float centroid in CardTagCentroidsX)  // Sum of List Values
                        sum_x += centroid;
                    CardCentroidTableTop.x = sum_x / CardTagCentroidsX.Count;  // Calc Average

                    float sum_z = 0;
                    foreach (float centroid in CardTagCentroidsZ)  // Sum of List Values
                        sum_z += centroid;
                    CardCentroidTableTop.z = sum_z / CardTagCentroidsZ.Count;  // Calc Average

                    isCardPositionCalibrated = true;  // Card Position Calibrated
                    CardTagCentroidsX.Clear();
                    CardTagCentroidsZ.Clear();
                    framesProcessed = 0;
                }
            }
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

        public void Notify()
        {
            Camera cam = cameraManager.ExternalCamera;
            if (cam == null)
                return;

            FrameData frameData = ArucoClient.Instance.Data;
            ArucoData tagData = frameData.ArucoData;

            if (tagData.Tags.Count > 0)
            {
                int frameHeight = frameData.ArucoData.height;
                int frameWidth = frameData.ArucoData.width;

                cam.aspect = (float)frameWidth / (float)frameHeight;
                cam.pixelRect = new UnityEngine.Rect(0, 0, frameWidth, frameHeight);
                cam.gateFit = Camera.GateFitMode.Vertical;


                Matrix4x4 M_CamIntrinsic = getIntrinsicCameraMatrix(frameData);  // Intrinsic Camera Projection:  3D Camera Space to 2D Screen Space.  Assumes camera position and rotation are zero and at the origin (no extrinsic information).
                Mat intrinsicCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
                for (int i = 0; i < intrinsicCamMatrix.rows(); i++)
                    for (int j = 0; j < intrinsicCamMatrix.cols(); j++)
                        intrinsicCamMatrix.put(i, j, M_CamIntrinsic[i, j]);

                MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);  // Distortion Coefficients are Zero for Simple Camera

                List<float> BatchCardTagCentroidsX = new List<float>();
                List<float> BatchCardTagCentroidsZ = new List<float>();
                foreach (Tag tag in tagData.Tags)
                {
                    int tagID = tag.getUniqueID();

                    float s;
                    if ((tagID % 28 == 26 || tagID % 28 == 27) && tagID <= 2911)  // Large Card Center Tags
                        s = 1f / 0.9f * DepthScalingFactor;
                    else if (tagID <= 2911)  // Small Tags Around the Perimeter of the Card
                        s = 0.3f / 0.9f * DepthScalingFactor;
                    else if (tagID >= 2912 && tagID <= 2946)  // 35 Tag Table Height Calibration Board
                        continue;  // skip
                    else  // Unknowns
                        s = DepthScalingFactor;

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


                    if (drawCardTags3D)  // Draw Tags in 3D Space
                    {
                        Debug.DrawLine(P_Camera[0], P_Camera[1], Color.green, persistance);
                        Debug.DrawLine(P_Camera[1], P_Camera[2], Color.green, persistance);
                        Debug.DrawLine(P_Camera[2], P_Camera[3], Color.green, persistance);
                        Debug.DrawLine(P_Camera[3], P_Camera[0], Color.green, persistance);
                    }

                    // Project Tag Points onto the Table Top
                    for (int i = 0; i < P_Camera.Length; i++)
                        P_Camera[i].y = tableTop.transform.position.y;

                    if (drawCardTagsTableTop)  // Draw Tags on Table Top Plane
                    {
                        Debug.DrawLine(P_Camera[0], P_Camera[1], Color.red, persistance);
                        Debug.DrawLine(P_Camera[1], P_Camera[2], Color.red, persistance);
                        Debug.DrawLine(P_Camera[2], P_Camera[3], Color.red, persistance);
                        Debug.DrawLine(P_Camera[3], P_Camera[0], Color.red, persistance);
                    }
                    
                    // Compute Centroid of Card based on Tag
                    Vector3 P_CardCentroidTableTop = M_ModelToCamera.MultiplyPoint3x4(CardCentroidModel);
                    P_CardCentroidTableTop.y = tableTop.transform.position.y;  // Project onto table top
                    Vector3 P_TagCentroidTableTop = M_ModelToCamera.MultiplyPoint3x4(Vector3.zero);
                    P_TagCentroidTableTop.y = tableTop.transform.position.y;

                    if (drawCardTagCentroids)  // Draw Tag Card Centroids
                        Debug.DrawLine(P_CardCentroidTableTop, P_TagCentroidTableTop, Color.blue, persistance);

                    // Add Tag Card Centroids to List for Processing Once Full
                    BatchCardTagCentroidsX.Add(P_CardCentroidTableTop.x);
                    BatchCardTagCentroidsZ.Add(P_CardCentroidTableTop.z);

                }

                // Add Tags to Collection for later averaging when a large enough sample has been collected
                if (!isCardPositionCalibrated && BatchCardTagCentroidsX.Count > 0 && BatchCardTagCentroidsZ.Count > 0)
                {
                    CardTagCentroidsX.AddRange(BatchCardTagCentroidsX);
                    CardTagCentroidsZ.AddRange(BatchCardTagCentroidsZ);
                    framesProcessed++;
                }
            }
        }
    }

}
