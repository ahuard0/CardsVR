using CardsVR.Networking;
using Photon.Pun;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class LocalCameraSelect : MonoBehaviour
    {

        #region Public Fields

        public GameObject ovrCameraRig1;
        public GameObject ovrCameraRig2;
        public GameObject ovrCameraRig3;
        public GameObject ovrCameraRig4;
        private bool _initialized = false;

        #endregion

        #region Monobehavior Callbacks

        void Update()
        {
            if (!_initialized)
            {
                int PlayerNumber = PlayerManager.Instance.PlayerNum;
                if (PlayerNumber == 4)
                {
                    if (ovrCameraRig1 != null)
                        ovrCameraRig1.SetActive(false);
                    if (ovrCameraRig2 != null)
                        ovrCameraRig2.SetActive(false);
                    if (ovrCameraRig3 != null)
                        ovrCameraRig3.SetActive(false);
                    if (ovrCameraRig4 != null)
                        ovrCameraRig4.SetActive(true);
                }
                if (PlayerNumber == 3)
                {
                    if (ovrCameraRig1 != null)
                        ovrCameraRig1.SetActive(false);
                    if (ovrCameraRig2 != null)
                        ovrCameraRig2.SetActive(false);
                    if (ovrCameraRig3 != null)
                        ovrCameraRig3.SetActive(true);
                    if (ovrCameraRig4 != null)
                        ovrCameraRig4.SetActive(false);
                }
                if (PlayerNumber == 2)
                {
                    if (ovrCameraRig1 != null)
                        ovrCameraRig1.SetActive(false);
                    if (ovrCameraRig2 != null)
                        ovrCameraRig2.SetActive(true);
                    if (ovrCameraRig3 != null)
                        ovrCameraRig3.SetActive(false);
                    if (ovrCameraRig4 != null)
                        ovrCameraRig4.SetActive(false);
                }
                else
                {
                    if (ovrCameraRig1 != null)
                        ovrCameraRig1.SetActive(true);
                    if (ovrCameraRig2 != null)
                        ovrCameraRig2.SetActive(false);
                    if (ovrCameraRig3 != null)
                        ovrCameraRig3.SetActive(false);
                    if (ovrCameraRig4 != null)
                        ovrCameraRig4.SetActive(false);
                }
                _initialized = true;
            }
        }

        #endregion
    }

}