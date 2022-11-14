using UnityEngine;
using CardsVR.Networking;
using CardsVR.Interaction;

namespace CardsVR.Commands
{
    public class HandsClient : MonoBehaviour
    {
        [Header("Remote Hands")]
        public GameObject RemoteHandLeft1;
        public GameObject RemoteHandRight1;
        public GameObject RemoteHandLeft2;
        public GameObject RemoteHandRight2;
        public GameObject RemoteHandLeft3;
        public GameObject RemoteHandRight3;
        public GameObject RemoteHandLeft4;
        public GameObject RemoteHandRight4;

        /*
         *      Retrieves and sends hand tracking base position and rotation data to remote clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void LateUpdate()
        {
            if (HandsManager.Instance.IsTrackHighConfidence(HandsManager.Side.Left))
            {
                int PlayerID = PlayerManager.Instance.PlayerNum;

                //PlayerID = 2; // DEBUG

                Transform LocalHandLeft = HandsManager.Instance.HandLeft.transform.parent;
                HandData msg = new HandData(PlayerID, HandData.HandSide.Left, LocalHandLeft.position, LocalHandLeft.rotation);

                SendData command = new SendData(data: msg, SendReliable: false, ReceiveLocally: false);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }

            if (HandsManager.Instance.IsTrackHighConfidence(HandsManager.Side.Right))
            {
                int PlayerID = PlayerManager.Instance.PlayerNum;

                //PlayerID = 2; // DEBUG

                Transform LocalHandRight = HandsManager.Instance.HandRight.transform.parent;
                HandData msg = new HandData(PlayerID, HandData.HandSide.Right, LocalHandRight.position, LocalHandRight.rotation);

                SendData command = new SendData(data: msg, SendReliable: false, ReceiveLocally: false);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }

        /*
         *      Method called by ConfigHandReceiver to set the Oculus OVR Hand configuration of remote players.
         *      
         *      Parameters
         *      ----------
         *      data : HandData
         *          The data object representing information needed to configure a custom OVR hand.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void SetHand(HandData data)
        {
            GameObject RemoteHandLeft;
            GameObject RemoteHandRight;
            if (data.PlayerID == 1)
            {
                RemoteHandLeft = RemoteHandLeft1;
                RemoteHandRight = RemoteHandRight1;
            }
            else if (data.PlayerID == 2)
            {
                RemoteHandLeft = RemoteHandLeft2;
                RemoteHandRight = RemoteHandRight2;
            }
            else if (data.PlayerID == 3)
            {
                RemoteHandLeft = RemoteHandLeft3;
                RemoteHandRight = RemoteHandRight3;
            }
            else if (data.PlayerID == 4)
            {
                RemoteHandLeft = RemoteHandLeft4;
                RemoteHandRight = RemoteHandRight4;
            }
            else 
                return;

            if (data.Side == HandData.HandSide.Left)
            {
                if (RemoteHandLeft != null)
                {
                    if (!RemoteHandLeft.activeSelf)
                        RemoteHandLeft.SetActive(true);
                    RemoteHandLeft.transform.position = data.BasePosition;
                    RemoteHandLeft.transform.rotation = data.BaseRotation;
                }
            }

            if (data.Side == HandData.HandSide.Right)
            {
                if (RemoteHandRight != null)
                {
                    if (!RemoteHandRight.activeSelf)
                        RemoteHandRight.SetActive(true);
                    RemoteHandRight.transform.position = data.BasePosition;
                    RemoteHandRight.transform.rotation = data.BaseRotation;
                }
            }
        }
    }
}
