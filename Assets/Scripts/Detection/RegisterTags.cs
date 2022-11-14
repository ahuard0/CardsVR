using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Calib3dModule;

namespace CardsVR.Detection
{
    public class RegisterTags : MonoBehaviour
    {

        public static ArucoData state = new ArucoData();

        public bool applyDepthCorrection;

        private Mat camMatrix;
        private MatOfDouble distCoeffs;

        public GameObject TagTemplate;

        public Camera VRCamera;

        private float screenDepth;
        private float screenNear;

        private GameObject PlayerTags;



        void Start()
        {
            // Get Container GO for Tags
            PlayerTags = GameObject.Find("PlayerTags");

            // Get Prefab Tags for Display
            //TagTemplate = GameObject.Find("TagTemplate");
            TagTemplate.SetActive(false);
            //CardTemplate = GameObject.Find("CardTemplate");
            //CardTemplate.SetActive(false);

            // Get Depth of Near and Far Screens
            screenDepth = VRCamera.farClipPlane;
            screenNear = VRCamera.nearClipPlane;


        }

        void ComputeCameraMatrix()
        {

            // Camera Matrix
            int max_d = (int)Mathf.Max(state.width, state.height);
            double fx = max_d;
            double fy = max_d;
            double cx = state.width / 2.0f;
            double cy = state.height / 2.0f;
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);

            // Distortion Coefficients are Zero for Simple Camera
            distCoeffs = new MatOfDouble(0, 0, 0, 0);
        }

        void Update()
        {

            // Clear Existing Tags
            foreach (Transform child in PlayerTags.transform)
            {
                GameObject go = child.gameObject;
                go.SetActive(false);
            }

            // Get Tags and Meta Info
            List<Tag> tags = state.Tags;
            double sensorWidth = state.width;
            double sensorHeight = state.height;

            // Process Each Tag
            double s = 0;
            foreach (Tag tag in tags)
            {

                // Get Tag Meta Information from Camera Far Plane
                double id = tag.getUniqueID();
                double dictId = tag.getDictID();
                Point3 centroid = tag.getCentroid3();
                MatOfPoint2f sensorPts = tag.getCorners2();
                MatOfPoint2f screenPts = new MatOfPoint2f();


                // Determine Depth Scaling
                double global_sf = 2f;
                if (applyDepthCorrection)
                {
                    if (id % 28 == 26 || id % 28 == 27)
                    {
                        s = 3.33f * global_sf;
                    }
                    else
                    {
                        s = 1f * global_sf;
                    }
                }
                else
                {
                    s = 1f * global_sf;
                }

                // Canonical Model of Tag for Display
                Point3[] cornersArray3 = new Point3[4];
                cornersArray3[0] = new Point3(0, 0, 0);
                cornersArray3[1] = new Point3(0, s, 0);
                cornersArray3[2] = new Point3(s, s, 0);
                cornersArray3[3] = new Point3(s, 0, 0);
                MatOfPoint3f modelPoints = new MatOfPoint3f(cornersArray3);
                Point3 modelCentroid = new Point3(s / 2, s / 2, 0);

                // Solve for 3D perspective using OpenCV
                ComputeCameraMatrix();
                Mat Rvec = new Mat();
                Mat Tvec = new Mat();
                Mat Rvec_aux = new Mat();
                Mat Tvec_aux = new Mat();

                // Pose Estimation -------------------------------------------------
                // Iterative PnP Solution is needed for stable results.  Other solver types cause instability and return failure on occasion. Iterative is recommended.
                bool success = Calib3d.solvePnP(modelPoints, sensorPts, camMatrix, distCoeffs, Rvec_aux, Tvec_aux, false, Calib3d.CV_ITERATIVE);
                if (!success)
                {
                    print("Could not solvePnP");
                    return;
                }

                Rvec_aux.convertTo(Rvec, CvType.CV_32F);
                Tvec_aux.convertTo(Tvec, CvType.CV_32F);

                Mat rotMat = new Mat(3, 3, CvType.CV_64FC1);
                Calib3d.Rodrigues(Rvec, rotMat);


                // Compute Transformation Matrix
                Matrix4x4 transformation = new Matrix4x4();
                transformation.SetRow(0, new Vector4((float)rotMat.get(0, 0)[0], (float)rotMat.get(0, 1)[0], (float)rotMat.get(0, 2)[0], (float)Tvec.get(0, 0)[0]));
                transformation.SetRow(1, new Vector4((float)rotMat.get(1, 0)[0], (float)rotMat.get(1, 1)[0], (float)rotMat.get(1, 2)[0], (float)Tvec.get(1, 0)[0]));
                transformation.SetRow(2, new Vector4((float)rotMat.get(2, 0)[0], (float)rotMat.get(2, 1)[0], (float)rotMat.get(2, 2)[0], (float)Tvec.get(2, 0)[0]));
                transformation.SetRow(3, new Vector4(0, 0, 0, 1));

                Rvec.Dispose();
                Tvec.Dispose();
                Rvec_aux.Dispose();
                Tvec_aux.Dispose();
                rotMat.Dispose();


                // Register The Tag with the Card Engine
                RegisteredTag newTag = new RegisteredTag(tag)
                {
                    transformation = transformation,
                    model = TagTemplate
                };
                this.DrawTag(newTag);
                RegisterCards.RegisterTagFrame(newTag);


                // Draw Ray on Mouse Press
                if (Input.GetMouseButtonDown(0))
                {
                    print("x: " + Input.mousePosition.x + ", y: " + Input.mousePosition.y);
                    Vector3 farPt = VRCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenDepth));
                    Vector3 nearPt = VRCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenNear));
                    Debug.DrawLine(nearPt, farPt, Color.green, 1f);
                }


            }

        }

        public void DrawTag(RegisteredTag regTag)
        {
            int id = regTag.getUniqueID();

            // Determine Depth Scaling
            double global_sf = 2f;
            double s;
            if (applyDepthCorrection)
            {
                if (id % 28 == 26 || id % 28 == 27)
                {
                    s = 3.33f * global_sf;
                }
                else
                {
                    s = 1f * global_sf;
                }
            }
            else
            {
                s = 1f * global_sf;
            }

            // draw tag
            GameObject Tag = Utility.Toolbox.FindInActiveObjectByName("Tag" + id);
            if (Tag == null)
            {
                Tag = Instantiate<GameObject>(TagTemplate, regTag.position, regTag.rotation, PlayerTags.transform);
                Tag.name = "Tag" + id;
            }
            else
            {
                Tag.transform.position = regTag.position;
                Tag.transform.rotation = regTag.rotation;
            }
            Tag.transform.localScale = new Vector3((float)s, (float)s, 1.0f);
            Tag.SetActive(true);
        }

    }

}

