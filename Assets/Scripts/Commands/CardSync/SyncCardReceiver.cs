using CardsVR.Interaction;
using CardsVR.Networking;
using CardsVR.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   SyncCardReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class SyncCardReceiver
    {
        public static int OwnerID;
        public static int[] CardIDs;
        public static int[] Piles;
        public static int[] PileIndex;
        public static CardSync.CardState[] States;
        public static Vector3[] Positions;
        public static Quaternion[] Rotations;

        /*
         *      Removes a card from an old stack and inserts it into another at
         *      the desired index.
         *      
         *      Parameters
         *      ----------
         *      CardID : int
         *          The ID number of the card.
         *      PileID_Current : int
         *          The PileID of the card to be moved.
         *      PileID_New : int
         *          The PileID of the card's destination.
         *      PileIndex_New : int
         *          The desired index of the card within the destination stack.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void UpdateStacks(int CardID, int PileID_Current, int PileID_New, int PileIndex_New)
        {
            // Remove card from Old Stack
            int[] RemoveStackArray = GameManager.Instance.getPileCardStack((int)PileID_Current).ToArray();
            Stack<int> RemoveStack = new Stack<int>();
            for (int j = 0; j < RemoveStackArray.Length; j++)
            {
                if (RemoveStackArray[j] != CardID)  // remove card from rebuilt stack
                    RemoveStack.Push(RemoveStackArray[j]);
            }
            GameManager.Instance.setPileCardStack((int)PileID_Current, RemoveStack);  // Replace old stack without card

            // Add card to the new Stack at the desired index
            int[] AddStackArray = GameManager.Instance.getPileCardStack(PileID_New).ToArray();
            Stack<int> AddStack = new Stack<int>();
            for (int j = 0; j < AddStackArray.Length; j++)
            {
                if (j != PileIndex_New)  // not the desired index : add the array value
                    AddStack.Push(AddStackArray[j]);
                else  // add the card at the desired index
                    AddStack.Push(CardID);
            }
            GameManager.Instance.setPileCardStack(PileID_New, AddStack);  // Replace old stack with card at desired index
        }

        public static Stack<int> RemoveDuplicates(Stack<int> Pile)
        {
            int[] newArray = Pile.ToArray();
            Stack<int> newStack = new Stack<int>();
            for (int j = 0; j < newArray.Length; j++)
            {
                int item = newArray[j];
                if (!newStack.Contains(item))
                    newStack.Push(item);
            }
            return newStack;
        }
        public static void RemoveDuplicates(int PileID)
        {
            Stack<int> Pile = GameManager.Instance.getPileCardStack(PileID);
            Pile = RemoveDuplicates(Pile);
            GameManager.Instance.setPileCardStack(PileID, Pile);
        }

        public static Stack<int> ReorderElementStack(Stack<int> Pile, int CardID, int Index)
        {
            // Remove card from Old Stack
            int[] oldArray = Pile.ToArray();
            List<int> newList = new List<int>();
            for (int j = 0; j < oldArray.Length; j++)
            {
                if (oldArray[j] != CardID)  // remove card from rebuilt stack
                    newList.Add(oldArray[j]);
            }

            // Add card to the new Stack at the desired index
            int[] newArray = newList.ToArray();
            List<int> orderedList = new List<int>();
            for (int j = 0; j < newArray.Length+1; j++)
            {
                if (j < Index)  // not the desired index : add the array value
                    orderedList.Add(newArray[j]);
                else if (j == Index)  // add the card at the desired index
                    orderedList.Add(CardID);
                else if (j > Index)
                    orderedList.Add(newArray[j-1]);
            }

            orderedList.Reverse();
            Stack<int> newStack = new Stack<int>(orderedList);

            return newStack;
        }
        public static void ReorderElement(int PileID, int CardID, int Index)
        {
            Stack<int> Pile = GameManager.Instance.getPileCardStack(PileID);
            Stack<int> NewPile = ReorderElementStack(Pile, CardID, Index);
            GameManager.Instance.setPileCardStack(PileID, NewPile);
        }

        

        /*
         *      Moves a card GameObject according to the instructions contained within the SyncCard command.
         *      
         *      Parameters
         *      ----------
         *      command : CardSync
         *          A command object representing the change in state of a card.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(CardSync command)
        {
            // Update Params
            CardIDs = command.CardIDs;
            Piles = command.Piles;
            PileIndex = command.PileIndex;
            States = command.States;
            Positions = command.Positions;
            Rotations = command.Rotations;
            OwnerID = command.OwnerID;

            if (OwnerID != PlayerManager.Instance.PlayerNum)  // Update non-owned cards.  Owned cards updated directly elsewhere.
            {
                for (int i = 0; i < CardIDs.Length; i++)  // add cards to stack
                {
                    int CardID = CardIDs[i];
                    int PileID = Piles[i];

                    int? PileNum_Current = GameManager.Instance.getPileNumByCard(CardID);  // The current PileID of the card

                    if (PileNum_Current == null)  // card not currently in a pile
                        PileNum_Current = PileID;

                    GameManager.Instance.removeCardFromPileStack((int)PileNum_Current, CardID);  // eliminates cards from prior stack if they exist
                    GameManager.Instance.addCardToPile(PileID, CardID); // duplicates will be eliminated during the later sorting process
                }

                for (int i = -3; i <= 7; i++)  // remove duplicates from all piles
                    RemoveDuplicates(i);

                for (int i = 0; i < CardIDs.Length; i++)  // sort
                {
                    int CardID = CardIDs[i];
                    int PileID = Piles[i];
                    int IndexPile = PileIndex[i];
                    CardSync.CardState State = States[i];
                    Vector3 Position = Positions[i];
                    Quaternion Rotation = Rotations[i];

                    ReorderElement(PileID, CardID, IndexPile);  // sort
                }

                // Assign Card Properties after import.  Must be an asychronous function because the deck may not yet be initialized.
                CardSyncClient client = GameObject.FindObjectOfType<CardSyncClient>();
                client.AssignCardProperties();  // Starts a coroutine to update the cards when the Deck objects become available.
            }
        }
    }
}
