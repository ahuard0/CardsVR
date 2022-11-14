using UnityEngine;
using CardsVR.Commands;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using CardsVR.Networking;

namespace CardsVR.Detection
{
    /*
     *      Operates the webcam and detects Aruco Tags in the Given Dictionaries.
     */
    public class ArucoClient : Singleton<ArucoClient>, IOnEventCallback, ISubject
    {
        private ArrayList _observers = new ArrayList();
        private FrameData _data;
        public FrameData Data
        {
            get
            { 
                return _data; 
            }
            set
            {
                _data = value;
                NotifyObservers();
            }
        }

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
         *      The client receives events and data from players on PUN, including ourself, using
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
                FrameData msg = FrameData.FromObjectArray(data);
                
                if (msg.PlayerID != PlayerManager.Instance.PlayerNum)  // Ignore Aruco Data from Other Players
                    return;

                Command command = new SyncFrame(msg);  // Process Aruco Data Frame
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(record: false);
            }
        }

        public void AttachObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void DetachObserver(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            try
            {
                foreach (IObserver observer in _observers)
                    observer.Notify();
            }
            catch (System.InvalidOperationException) { }  // Occurs when observers are added or detached while the subject is inside the foreach loop.  This try/catch block acts as a break for the loop when this event occurs.
        }
    }
}
