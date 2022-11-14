using CardsVR.Interaction;
using CardsVR.Networking;
using CardsVR.States;
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
    public class CardSyncClient : MonoBehaviour, IOnEventCallback
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

        /*
         *      Unity MonoBehavior callback called on startup.
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
            int OwnerID = 1;
            if (PlayerManager.Instance.PlayerNum != 1)  // Master client deals the deck.  Others synchronize with Player 1.
                BroadcastCardDataRequest(OwnerID);
        }

        private void BroadcastCardDataRequest(int OwnerID)
        {
            int RequesterID = PlayerManager.Instance.PlayerNum;

            RequestCardData data = new RequestCardData(OwnerID, RequesterID);
            SendData command = new SendData(data: data, SendReliable: true, ReceiveLocally: false);
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(false);  // do not record command history
        }

        public void AssignCardProperties()
        {
            StartCoroutine("WaitAndAssignCardProperties");
        }

        public IEnumerator WaitAndAssignCardProperties()
        {
            

            for (int i = 0; i < SyncCardReceiver.CardIDs.Length; i++)  // sort and assign properties
            {

                int CardID = SyncCardReceiver.CardIDs[i];
                int PileID = SyncCardReceiver.Piles[i];
                int IndexPile = SyncCardReceiver.PileIndex[i];
                CardSync.CardState State = SyncCardReceiver.States[i];
                Vector3 Position = SyncCardReceiver.Positions[i];
                Quaternion Rotation = SyncCardReceiver.Rotations[i];

                GameObject card = GameManager.Instance.getCardByTag(CardID);  // Attempt to get the game object (card)
                if (card == null)  // Game object (card) not initialized
                {
                    yield return new WaitUntil(() => GameManager.Instance.getCardByTag(CardID) != null);  // Wait until Game Object (card) initialized
                    card = GameManager.Instance.getCardByTag(CardID);  // Get the game object (card)
                }
                    
                CardStateContext context = card.GetComponent<CardStateContext>();  // get the state machine context
                if (context == null)
                    Debug.LogError("Could not find state context.");

                if (State == CardSync.CardState.Pile)  // owner's card state is "Pile" -> remote users should also have this card in the pile state to match the owner
                {
                    GameObject pileAnchor = GameManager.Instance.getPileCardAnchor(PileID);
                    if (pileAnchor == null)
                        Debug.LogErrorFormat("Pile Num ({0}) Anchor not found.", PileID);

                    card.transform.parent = pileAnchor.transform;  // attach card to the pile anchor
                    context.PileNum = PileID;  // update the context PileID

                    if (context.currentState != context.pileState)  // check state machine context state and adjust if necessary
                        context.ChangeState(context.pileState);
                }
                else if (State == CardSync.CardState.Finger)  // Owner is "Finger" state -> Remote users will use the "Move" state.
                {
                    int NewPileID = -3;  // Set to Free Card container (Pile "-3")

                    GameObject pileAnchor = GameManager.Instance.getPileCardAnchor(NewPileID);  // Set to Free Card container.  
                    if (pileAnchor == null)
                        Debug.LogErrorFormat("Pile Num ({0}) Anchor not found.", NewPileID);

                    card.transform.parent = pileAnchor.transform;  // attach card to the pile anchor
                    context.PileNum = NewPileID;  // update the context PileID

                    if (context.currentState != context.moveState)  // check state machine context state and adjust if necessary
                        context.ChangeState(context.moveState);
                }

                context.Position = Position;  // context may adjust position & rotation internally in the state machine
                context.Rotation = Rotation;

                card.transform.position = Position;  // set position & rotation anyway just in case
                card.transform.rotation = Rotation;
            }
        }

        /*
         *      Sync the card model data across clients.
         *      
         *      Used to recover position, pile, and rotation data if ownership is lost.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void BroadcastCardData(int OwnerID)
        {
            List<int> CardIDs = GameManager.Instance.CardsOwned(OwnerID);

            List<int> Piles = new List<int>();
            List<int> PileIndex = new List<int>();
            List<CardRecover.CardState> States = new List<CardRecover.CardState>();
            List<Vector3> Positions = new List<Vector3>();
            List<Quaternion> Rotations = new List<Quaternion>();

            foreach (int CardID in CardIDs)
            {
                int? PileNum = GameManager.Instance.getPileNumByCard(CardID);
                if (PileNum == null)
                    Piles.Add(-3);  // free card (no pile)
                else
                    Piles.Add((int)PileNum);

                int? Index = GameManager.Instance.getPileIndexByCard(CardID);
                if (Index == null)
                    PileIndex.Add(-1);
                else
                    PileIndex.Add((int)Index);

                GameObject card = GameManager.Instance.getCardByTag(CardID);  // Get the game object
                if (card == null)
                    return;  // Deck may not yet be initialized

                CardStateContext context = card.GetComponent<CardStateContext>();  // get the state machine context
                if (context == null)
                    Debug.LogError("Could not find state context.");

                if (context.currentState == context.pileState)
                    States.Add(CardRecover.CardState.Pile);
                else if (context.currentState == context.fingerState)
                    States.Add(CardRecover.CardState.Finger);
                else if (context.currentState == context.moveState)
                    States.Add(CardRecover.CardState.Move);

                Positions.Add(card.transform.position);
                Rotations.Add(card.transform.rotation);
            }

            if (CardIDs.Count > 0)
            {
                CardRecover data = new CardRecover(CardIDs.ToArray(), Piles.ToArray(), PileIndex.ToArray(), States.ToArray(), Positions.ToArray(), Rotations.ToArray(), OwnerID);
                SendData command = new SendData(data: data, SendReliable: false, ReceiveLocally: true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }

        /*
         *      Sync the card model data across clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void BroadcastOwnedCardData()
        {
            int OwnerID = PlayerManager.Instance.PlayerNum;
            List<int> CardIDs = GameManager.Instance.CardsOwned(OwnerID);

            List<int> Piles = new List<int>();
            List<int> PileIndex = new List<int>();
            List<CardSync.CardState> States = new List<CardSync.CardState>();
            List<Vector3> Positions = new List<Vector3>();
            List<Quaternion> Rotations = new List<Quaternion>();

            foreach (int CardID in CardIDs)
            {
                int? PileNum = GameManager.Instance.getPileNumByCard(CardID);
                if (PileNum == null)
                    Piles.Add(-3);  // free card (no pile)
                else
                    Piles.Add((int)PileNum);

                int? Index = GameManager.Instance.getPileIndexByCard(CardID);
                if (Index == null)
                    PileIndex.Add(-1);
                else
                    PileIndex.Add((int)Index);

                GameObject card = GameManager.Instance.getCardByTag(CardID);  // Get the game object
                if (card == null)
                    return;  // Deck may not yet be initialized

                CardStateContext context = card.GetComponent<CardStateContext>();  // get the state machine context
                if (context == null)
                    Debug.LogError("Could not find state context.");

                if (context.currentState == context.pileState)
                    States.Add(CardSync.CardState.Pile);
                else if (context.currentState == context.fingerState)
                    States.Add(CardSync.CardState.Finger);
                else if (context.currentState == context.moveState)
                    States.Add(CardSync.CardState.Move);

                Positions.Add(card.transform.position);
                Rotations.Add(card.transform.rotation);
            }

            if (CardIDs.Count > 0)
            {
                CardSync data = new CardSync(CardIDs.ToArray(), Piles.ToArray(), PileIndex.ToArray(), States.ToArray(), Positions.ToArray(), Rotations.ToArray(), OwnerID);
                SendData command = new SendData(data: data, SendReliable: false, ReceiveLocally: true);
                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(true);  // record command history
            }
        }

        /*
         *      PUN uses this callback method to respond to PUN events. The client must first be 
         *      registered to receive PUN events.
         *      
         *      CardSyncClient receives events and data from players on PUN, including ourself, using
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
            if (photonEvent.Code == CardControlData.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                CardControlData msg = CardControlData.FromObjectArray(data);
                Command command = new RequestCardControl(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(false);  // do not record command history
            }
            else if (photonEvent.Code == CardSync.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                CardSync msg = CardSync.FromObjectArray(data);
                Command command = new SyncCard(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(false);  // do not record command history
            }
            else if (photonEvent.Code == CardRecover.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                CardRecover msg = CardRecover.FromObjectArray(data);
                Command command = new RecoverCard(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(false);  // do not record command history
            }
            else if (photonEvent.Code == RequestCardData.EventID)
            {
                object[] data = (object[])photonEvent.CustomData;
                RequestCardData msg = RequestCardData.FromObjectArray(data);
                Command command = new RequestCard(msg);

                Invoker.Instance.SetCommand(command);
                Invoker.Instance.ExecuteCommand(false);  // do not record command history
            }
        }
    }
}
