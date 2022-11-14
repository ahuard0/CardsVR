using CardsVR.Commands;
using CardsVR.Interaction;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using CardsVR.Utility;

namespace CardsVR.Detection
{
    public class DrawTagsBoard3D : MonoBehaviour, IObserver
    {
        [Header("Camera Manager")]
        [SerializeField]
        private CameraManager cameraManager;

        [Header("Parameters")]
        [SerializeField]
        private float persistance = 1f;
        [SerializeField]
        private int[] SearchArucoIDs;
        [SerializeField]
        private float s = 1f;


        private void OnEnable()
        {
            ArucoClient.Instance.AttachObserver(this);
        }

        private void OnDisable()
        {
            ArucoClient.Instance.DetachObserver(this);
        }

        private Matrix4x4 getIntrinsicCameraMatrix(ArucoData tagData)
        {
            int max_d = (int)Mathf.Max(tagData.width, tagData.height);
            float fx = max_d;
            float fy = max_d;
            float cx = tagData.width / 2.0f;
            float cy = tagData.height / 2.0f;
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


            int frameHeight = frameData.ArucoData.height;
            int frameWidth = frameData.ArucoData.width;

            cam.aspect = (float)frameWidth / (float)frameHeight;
            cam.pixelRect = new UnityEngine.Rect(0, 0, frameWidth, frameHeight);
            cam.gateFit = Camera.GateFitMode.Vertical;


            Matrix4x4 M_CamIntrinsic = getIntrinsicCameraMatrix(tagData);  // Intrinsic Camera Projection:  3D Camera Space to 2D Screen Space.  Assumes camera position and rotation are zero and at the origin (no extrinsic information).
            Mat intrinsicCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
            for (int i = 0; i < intrinsicCamMatrix.rows(); i++)
                for (int j = 0; j < intrinsicCamMatrix.cols(); j++)
                    intrinsicCamMatrix.put(i, j, M_CamIntrinsic[i, j]);

            MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);  // Distortion Coefficients are Zero for Simple Camera

            foreach (Tag tag in tagData.Tags)
            {
                int tagID = tag.getUniqueID();
                if (!Toolbox.Exists<int>(SearchArucoIDs, tagID))  // tag ID must be in search group
                    continue;

                MatOfPoint2f sensorPoints = tag.getCorners2();


                //  Model Coords : Canonical Model of Tag
                Vector3[] modelPts = new Vector3[4];
                modelPts[0] = new Vector3(-0.5f * s, 0, -0.5f * s);
                modelPts[1] = new Vector3( 0.5f * s, 0, -0.5f * s);
                modelPts[2] = new Vector3( 0.5f * s, 0,  0.5f * s);
                modelPts[3] = new Vector3(-0.5f * s, 0,  0.5f * s);

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



                Debug.DrawLine(P_Camera[0], P_Camera[1], Color.green, persistance);
                Debug.DrawLine(P_Camera[1], P_Camera[2], Color.green, persistance);
                Debug.DrawLine(P_Camera[2], P_Camera[3], Color.green, persistance);
                Debug.DrawLine(P_Camera[3], P_Camera[0], Color.green, persistance);

                Rvec.Dispose();
                Tvec.Dispose();
                Rvec_aux.Dispose();
                Tvec_aux.Dispose();
                rotMat.Dispose();


            }
        }
    }

}
