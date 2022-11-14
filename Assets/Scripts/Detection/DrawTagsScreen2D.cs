using CardsVR.Commands;
using CardsVR.Interaction;
using OpenCVForUnity.CoreModule;
using UnityEngine;

namespace CardsVR.Detection
{
    public class DrawTagsScreen2D : MonoBehaviour, IObserver
    {
        [Header("Camera Manager")]
        [SerializeField]
        private CameraManager cameraManager;

        [Header("Parameters")]
        [SerializeField]
        private float persistance = 0.2f;

        private void OnEnable()
        {
            ArucoClient.Instance.AttachObserver(this);
        }

        private void OnDisable()
        {
            ArucoClient.Instance.DetachObserver(this);
        }

        public void Notify()
        {
            Camera cam = cameraManager.ExternalCamera;
            if (cam == null)
                return;

            FrameData frameData = ArucoClient.Instance.Data;
            ArucoData tagData = frameData.ArucoData;

            int sensorHeight = tagData.height;
            int sensorWidth = tagData.width;

            int frameHeight = frameData.ArucoData.height;
            int frameWidth  = frameData.ArucoData.width;

            cam.aspect = (float)frameWidth / (float)frameHeight;
            cam.pixelRect = new UnityEngine.Rect(0, 0, frameWidth, frameHeight);
            cam.gateFit = Camera.GateFitMode.Vertical;

            int screenWidth = cam.pixelWidth;
            int screenHeight = cam.pixelHeight;
            float depth = cam.farClipPlane;

            foreach (Tag tag in tagData.Tags)
            {
                MatOfPoint2f imgPoints = tag.getCorners2();
                Point[] pt = imgPoints.toArray();

                float screen_x0 = (float)(screenWidth  * pt[0].x / sensorWidth);
                float screen_y0 = (float)(screenHeight * pt[0].y / sensorHeight);
                float screen_x1 = (float)(screenWidth  * pt[1].x / sensorWidth);
                float screen_y1 = (float)(screenHeight * pt[1].y / sensorHeight);
                float screen_x2 = (float)(screenWidth  * pt[2].x / sensorWidth);
                float screen_y2 = (float)(screenHeight * pt[2].y / sensorHeight);
                float screen_x3 = (float)(screenWidth  * pt[3].x / sensorWidth);
                float screen_y3 = (float)(screenHeight * pt[3].y / sensorHeight);

                Vector3 world_pt0 = cam.ScreenToWorldPoint(new Vector3(screen_x0, screen_y0, depth));
                Vector3 world_pt1 = cam.ScreenToWorldPoint(new Vector3(screen_x1, screen_y1, depth));
                Vector3 world_pt2 = cam.ScreenToWorldPoint(new Vector3(screen_x2, screen_y2, depth));
                Vector3 world_pt3 = cam.ScreenToWorldPoint(new Vector3(screen_x3, screen_y3, depth));

                Debug.DrawLine(world_pt0, world_pt1, Color.magenta, persistance);
                Debug.DrawLine(world_pt1, world_pt2, Color.magenta, persistance);
                Debug.DrawLine(world_pt2, world_pt3, Color.magenta, persistance);
                Debug.DrawLine(world_pt3, world_pt0, Color.magenta, persistance);

            }
        }
    }

}
