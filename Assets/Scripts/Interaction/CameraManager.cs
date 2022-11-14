using CardsVR.Networking;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class CameraManager : MonoBehaviour
    {
        [Header("OVR Cameras")]
        public OVRCameraRig OVRCamera;
        public GameObject Player1;
        public GameObject Player2;
        public GameObject Player3;
        public GameObject Player4;

        [Header("External Cameras")]
        public Camera ExternalCamera;
        public float ExtCamYOffset = 0;  // e.g., -0.1 (Camera Height Correction)
        public float ExtCamXOffset = 0;  // e.g., -0.1 (Camera Side-to-Side Correction)
        public float ExtCamZOffset = 0;  // e.g., 0 (Camera Depth Correction)

        private void Update()
        {
            ExternalCamera.transform.localPosition = new Vector3(ExtCamXOffset, ExtCamYOffset, ExtCamZOffset);

            int PlayerNumber = PlayerManager.Instance.PlayerNum;
            if (PlayerNumber == 4)
                OVRCamera.transform.parent = Player4.transform;
            if (PlayerNumber == 3)
                OVRCamera.transform.parent = Player3.transform;
            if (PlayerNumber == 2)
                OVRCamera.transform.parent = Player2.transform;
            else
                OVRCamera.transform.parent = Player1.transform;
            OVRCamera.transform.localPosition = Vector3.zero;
            OVRCamera.transform.localRotation = Quaternion.identity;
        }
    }
}