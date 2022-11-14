using CardsVR.Interaction;
using CardsVR.Networking;
using CardsVR.States;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   RecoverCardReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class RecoverCardReceiver
    {
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

        /*
         *      Moves a card GameObject according to the instructions contained within the RecoverCard command.
         *      
         *      Parameters
         *      ----------
         *      command : CardRecover
         *          A command object representing the change in state of a card.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(CardRecover command)
        {
            int[] CardIDs = command.CardIDs;
            int[] Piles = command.Piles;
            int[] PileIndex = command.PileIndex;
            CardRecover.CardState[] States = command.States;
            Vector3[] Positions = command.Positions;
            Quaternion[] Rotations = command.Rotations;
            int OwnerID = command.OwnerID;

            GameManager GM = GameManager.Instance;

            if (OwnerID == PlayerManager.Instance.PlayerNum)  // Update cards that should be owned by the current user.
            {
                for (int i = 0; i < CardIDs.Length; i++)
                {
                    int CardID = CardIDs[i];

                    if (!GameManager.Instance.isCardUnowned(CardID))  // only recover locally unowned cards (that should be owned by the current user)
                        return;
                    else
                        GM.setCardOwned(CardID, PlayerManager.Instance.PlayerNum);  // update ownership local flag, set card properties below to recover the card

                    int PileID = Piles[i];
                    int IndexPile = PileIndex[i];
                    CardRecover.CardState State = States[i];
                    Vector3 Position = Positions[i];
                    Quaternion Rotation = Rotations[i];

                    GameObject card = GameManager.Instance.getCardByTag(CardID);  // Get the game object
                    if (card == null)
                        Debug.LogErrorFormat("Card not found: {0}", CardID);

                    CardStateContext context = card.GetComponent<CardStateContext>();  // get the state machine context
                    if (context == null)
                        Debug.LogError("Could not find state context.");

                    int? PileNum_Current = GM.getPileNumByCard(CardID);  // The current PileID of the card (to be updated)
                    int? PileIndex_Current = GM.getPileIndexByCard(CardID);  // The current Pile index of the card within the current stack (to be updated)

                    if (PileNum_Current == null)  // pile not initialized -> add to pile
                    {
                        GameManager.Instance.addCardToPile(PileID, CardID);
                        PileNum_Current = PileID;
                    }

                    if (State == CardRecover.CardState.Pile)  // owner's card state is "Pile" -> remote users should also have this card in the pile state to match the owner
                    {
                        if (PileNum_Current != PileID || PileIndex_Current != IndexPile)  // Pile number or index within the pile are inconsistent
                        {
                            GameObject pileAnchor = GameManager.Instance.getPileCardAnchor(PileID);
                            if (pileAnchor == null)
                                Debug.LogErrorFormat("Pile Num ({0}) Anchor not found.", PileID);

                            card.transform.parent = pileAnchor.transform;  // attach card to the pile anchor
                            context.PileNum = PileID;  // update the context PileID

                            UpdateStacks(CardID, (int)PileNum_Current, PileID, IndexPile);  // Updates the stack data model
                        }

                        if (context.currentState != context.pileState)  // check state machine context state and adjust if necessary
                            context.ChangeState(context.pileState);

                        context.Position = Position;  // context may adjust position & rotation internally in the state machine
                        context.Rotation = Rotation;

                        card.transform.position = Position;  // set position & rotation anyway just in case
                        card.transform.rotation = Rotation;
                    }
                    else if (State == CardRecover.CardState.Finger)  // Owner is "Finger" state -> Remote users will use the "Move" state.
                    {
                        int NewPileID = -3;  // Set to Free Card container (Pile "-3")

                        GameObject pileAnchor = GameManager.Instance.getPileCardAnchor(NewPileID);  // Set to Free Card container.  
                        if (pileAnchor == null)
                            Debug.LogErrorFormat("Pile Num ({0}) Anchor not found.", NewPileID);

                        card.transform.parent = pileAnchor.transform;  // attach card to the pile anchor
                        context.PileNum = NewPileID;  // update the context PileID

                        UpdateStacks(CardID, (int)PileNum_Current, PileID, IndexPile);  // Updates the stack data model

                        if (context.currentState != context.moveState)  // check state machine context state and adjust if necessary
                            context.ChangeState(context.moveState);

                        context.Position = Position;  // context may adjust position & rotation internally in the state machine
                        context.Rotation = Rotation;

                        card.transform.position = Position;  // set position & rotation anyway just in case
                        card.transform.rotation = Rotation;
                    }
                }
            }
        }
    }
}
