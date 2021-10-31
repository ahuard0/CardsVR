using CardsVR.Interaction;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace CardsVR.Commands
{
    public class DataUnitTest : MonoBehaviour, IOnEventCallback
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
            else
                return;

            int count_initial = CommandRecorder.Instance.Count();

            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history

            int count_new = CommandRecorder.Instance.Count();

            if (count_new <= count_initial)
                Debug.LogError("Command count did not increase.  DataClient fails unit test.");
            if (count_new <= count_initial)
                Debug.Log("Command count increased by 1 as expected.  DataClient passes unit test.");
        }

        /*
         *      Unity callback for overlay GUI elements (e.g., buttons).
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnGUI()
        {
            if (GUILayout.Button("Message"))
            {
                // Send to Remote clients
                Message msg = new Message("This is a test.");
                SendData command = new SendData(msg, false, true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }

            if (GUILayout.Button("Movement"))
            {
                GameObject card = GameManager.Instance.getCardByTag(5);
                Movement movement = new Movement(card.name, card.transform.position, card.transform.rotation, Movement.Type.Absolute);
                Vector3 position = movement.Pos;
                position.x += 0.05f;
                movement.Pos = position;

                SendData command = new SendData(movement, false, true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }

            if (GUILayout.Button("Card to Pile"))
            {
                int cardID = Mathf.RoundToInt(Random.Range(0, 52));
                int pileID = Mathf.RoundToInt(Random.Range(0, 7));
                GameObject card = GameManager.Instance.getCardByTag(cardID);
                CardToPile data = new CardToPile(cardID, pileID, card.name);

                SendData command = new SendData(data, false, true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }
    }
}
