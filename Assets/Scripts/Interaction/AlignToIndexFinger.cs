using UnityEngine;

namespace CardsVR.Interaction
{
    public class AlignToIndexFinger : MonoBehaviour
    {
        private HandsManager HM;

        private void Start()
        {
            HM = HandsManager.Instance;
        }

        public void Update()
        {
            if (HM.DominantIndexTip != null)
            {
                // Track Finger Position
                this.transform.position = HM.DominantIndexTip.Transform.position;

                // Rotate Card to face user
                Vector3 rot = HM.DominantIndexTip.Transform.localEulerAngles;
                rot.y += 90;
                rot.z += 180;
                this.transform.localEulerAngles = rot;
            }
        }
    }

}

