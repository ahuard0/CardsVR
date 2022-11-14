using CardsVR.Interaction;
using CardsVR.States;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   RequestCardReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class RequestCardReceiver
    {
        /*
         *      Initiates a broadcast in card state.
         *      
         *      Parameters
         *      ----------
         *      command : RequestCard
         *          A command object representing the broadcast request.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(RequestCardData command)
        {
            int OwnerID = command.OwnerID;
            int RequesterID = command.RequesterID;

            BroadcastCardData(OwnerID, RequesterID);
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
        private static void BroadcastCardData(int OwnerID, int RequesterID)
        {
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
    }
}
