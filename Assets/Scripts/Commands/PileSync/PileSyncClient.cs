using CardsVR.Interaction;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   PileSyncClient follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public class PileSyncClient : MonoBehaviour, IOnEventCallback
    {
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

        private void Start()
        {
            StartCoroutine("SyncPiles");
        }

        /*
         *      A coroutine used to sync the pile model data across clients with a time 
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
        private IEnumerator SyncPiles()
        {
            int i = 0;
            while(true)
            {
                SyncPileData(i);
                i++;
                if (i > 6)
                    i = 0;
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        /*
         *      Sync the pile model data across clients.
         *      
         *      Parameters
         *      ----------
         *      pileID : int
         *          The unique ID of the pile to sync.
         *      
         *      Returns
         *      -------
         *      None
         */
        private void SyncPileData(int pileID)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Stack<int> cardIDs = GameManager.Instance.getPileCardStack(pileID);
                PileData data = new PileData(pileID, cardIDs);
                SendData command = new SendData(data, false, true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }

        /*
         *      PUN uses this callback method to respond to PUN events. The client must first be 
         *      registered to receive PUN events.
         *      
         *      PileSyncClient receives events and data from players on PUN, including ourself, using
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
            if (photonEvent.Code == PileData.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                PileData msg = PileData.FromObjectArray(data);
                Command command = new SyncPile(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }
    }
}
