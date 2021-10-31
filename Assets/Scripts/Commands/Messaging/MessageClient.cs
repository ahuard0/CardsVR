using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace CardsVR.Commands
{
    /*
     *      MessageClient handles the sending and receiving of Messages, 
     *      registers callbacks, and acts as the nexus of the messaging 
     *      protocol.
     *      
     *      This MonoBehavior component should be attached to a GameObject
     *      in the scene.  
     *      
     *      IOnEventCallback is implemented to receive PUN Events using the
     *      OnEvent callback method.
     *      
     *      IMessageClient is implemented to provide the SetMessage callback
     *      method for MessageReceiver.
     */
    public class MessageClient : MonoBehaviour, IOnEventCallback
    {
        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the MessageClient with the Message Reciever and
         *      registers the MessageClient to receive PUN events.
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
         *      This method unregisters the MessageClient with the Message Reciever and
         *      unregisters the MessageClient from receiving PUN events.
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
         *      This callback is attached to a button UI element in the Unity inspector.
         *      It serves as the main trigger for sending a Message in this application.
         *      
         *      This function should be tailored to the application UI.
         *      
         *      Parameters
         *      ----------
         *      input : Text
         *          Use the Unity Inspector to set the Text field where string content will
         *          be populated into a Message object and broadcast other clients on PUN.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void BroadcastMessageBtnCallback(Text input)
        {
            string message_str = input.text;  // Get String Message from Text Input Field

            // Create Message Object
            Message msg = new Message(message_str);

            // Create Command
            SendMessage command = new SendMessage(msg);

            // Invoke Command
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
        }

        /*
         *      PUN uses this callback method to respond to PUN events. MessageClient must first be 
         *      registered to receive PUN events.
         *      
         *      MessageClient receives Message data from players on PUN, including ourself, using
         *      the OnEvent callback.  For example, invoking the SendMessage command will trigger
         *      the OnEvent callback, which triggers a ReceiveMessage command for all players
         *      regardless of who sent the message in the first place.
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
            byte EventCode = photonEvent.Code;

            if (EventCode == Message.EventID)
            {
                // Create Message Object
                object[] data = (object[])photonEvent.CustomData;
                Message msg = Message.FromObjectArray(data);

                // Create Command
                ReceiveMessage command = new ReceiveMessage(msg);

                // Invoke Command
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }
    }
}
