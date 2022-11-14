using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   FrameClient follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public class FrameClient : MonoBehaviour, IOnEventCallback
    {
        [SerializeField]
        private float _frameSyncInterval = 0.2f;

        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the client to receive PUN events.
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
         *      This method unregisters the client from receiving PUN events.
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
         *      Unity MonoBehavior callback used to perform actions on startup.
         *      
         *      This method starts the coroutine that synchronizes the card Frames
         *      periodically, one Frame every fixed time period.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */

        private void Start()
        {
            StartCoroutine("SyncFrames");
        }

        /*
         *      A coroutine used to sync the Frame model data across clients with a time 
         *      delay between sync operations.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Coroutine exec called separately.")]
        private IEnumerator SyncFrames()
        {
            while(true)
            {
                SyncFrameData();
                yield return new WaitForSecondsRealtime(_frameSyncInterval);
            }
        }

        /*
         *      Sync the Frame model data across clients.
         *      
         *      Parameters
         *      ----------
         *      FrameID : int
         *          The unique ID of the Frame to sync.
         *      
         *      Returns
         *      -------
         *      None
         */
        private void SyncFrameData()
        {
            //Stack<int> cardIDs = GameManager.Instance.getFrameCardStack(FrameID);
            //FrameData data = new FrameData(FrameID, cardIDs);
            //SendData command = new SendData(data: data, SendReliable: false, ReceiveLocally: true);
            //Invoker.Instance.SetCommand(command);
            //Invoker.Instance.ExecuteCommand(true);  // record command history
        }

        /*
         *      PUN uses this callback method to respond to PUN events. The client must first be 
         *      registered to receive PUN events.
         *      
         *      FrameClient receives events and data from players on PUN, including ourself, using
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
            if (photonEvent.Code == FrameData.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                //FrameData msg = FrameData.FromObjectArray(data);
                //Command command = new SyncFrame(msg);

                //Invoker.Instance.SetCommand(command);
                //Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }
    }
}
