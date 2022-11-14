using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   DataClient follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   DataClient is called by the Execute method of the ReceiveMovement class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public class DataClient : Singleton<DataClient>, IOnEventCallback
    {
        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the MessageClient to receive PUN events.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /*
         *      Unity MonoBehavior callback OnDisable is called whenever the attached 
         *      GameObject is disabled.  This occurs when quitting the program or
         *      loading another scene.
         *      
         *      This method unregisters the MessageClient from receiving PUN events.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        /*
         *      PUN uses this callback method to respond to PUN events. The client must first be 
         *      registered to receive PUN events.
         *      
         *      DataClient receives events and data from players on PUN, including ourself, using
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
            byte EventID = photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            Command command;
            if (EventID == CardToPile.EventID)
            {
                CardToPile msg = CardToPile.FromObjectArray(data);
                command = new MoveCardToPile(msg);
            }
            else if (EventID == HandData.EventID)
            {
                HandData msg = HandData.FromObjectArray(data);
                command = new ConfigHand(msg);
            }
            else if (EventID == AvatarData.EventID)
            {
                AvatarData msg = AvatarData.FromObjectArray(data);
                command = new ConfigAvatar(msg);
            }
            else if (EventID == Message.EventID)
            {
                Message msg = Message.FromObjectArray(data);
                command = new ReceiveMessage(msg);
            }
            else if (EventID == Movement.EventID)
            {
                Movement msg = Movement.FromObjectArray(data);
                command = new ReceiveMovement(msg);
            }
            else if (EventID == TableHeightData.EventID)
            {
                TableHeightData msg = TableHeightData.FromObjectArray(data);
                command = new SyncTableHeight(msg);
            }
            else
                return;

            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
        }
    }
}
