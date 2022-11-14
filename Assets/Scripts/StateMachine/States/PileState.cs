using CardsVR.Commands;
using CardsVR.Interaction;
using CardsVR.Networking;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.States
{
    public class PileState : BaseState, IObserver
    {
        private CardStateContext Context;

        private bool topCard;  // only the top most card is observed
        private bool initialized;  // ensure at least one logic update has been performed before processing events

        public PileState(CardStateContext stateMachine) : base("PileState", stateMachine)
        {
            Context = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            initialized = false;  // at least one logic update cycle must be performed before processing Notify() events
            DominantHandCollisionDetector.Instance.AttachObserver(this);
        }

        public override void Exit()
        {
            base.Exit();
            DominantHandCollisionDetector.Instance.DetachObserver(this);
        }

        private bool Validate()
        {
            // SET CONTEXT PARAMETERS
            // Update Context PileID
            int? pileID = GameManager.Instance.getPileNumByCard(Context.CardID);
            if (pileID == null)
                return false;
            else
                Context.PileNum = (int)pileID;

            // Update State if PileNum doesn't match
            if (Context.PileNum == -1)  // Race condition detected, change state to finger state
            {
                Context.ChangeState(Context.fingerState);
                return false;
            }

            return true;
        }

        /*
         *      Synchronizes the card's Game Object with the GameManager and context model.
         *      
         *      This method acts as the "view" in the Model/View design pattern.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (!Validate())
                return;

            // SET STATE PARAMETERS
            // Update State initialized Flag
            initialized = true;  // at least one logic update cycle must be performed before processing Notify() events

            // Update State TopCard Flag
            topCard = Context.CardID == GameManager.Instance.peekCardID(Context.PileNum);  // only pick up cards that are on the top of each pile stack.  Used in Notify().

            // Update Context FaceUp Flag
            if (Context.PileNum == 0 || Context.PileNum == 7)  // Card is in a Deck Pile (0 or 7)
                Context.FaceUp = false;
            else  // Card is in a Table Pile
                Context.FaceUp = true;

            // CONFIGURE GAME OBJECT
            // Get the Card GO
            GameObject card = GameManager.Instance.getCardByTag(Context.CardID);
            if (card == null)
                Debug.LogErrorFormat("Card not found.  ID={0}.", Context.CardID);

            // Set the Card GO Parent
            GameObject cardAnchor = GameManager.Instance.getPileCardAnchor(Context.PileNum);
            if (cardAnchor == null)
                return;
            card.transform.parent = cardAnchor.transform;

            // Set Card GO Position
            int? i = GameManager.Instance.getStackPosition(Context.CardID, Context.PileNum);  // Get the position within the stack
            if (i == null)
                Debug.LogError("Card not found in the pile stack!");

            // Get the Number of Cards in the Stack
            int Pile_Count = GameManager.Instance.getNumCards(Context.PileNum);
            if (Pile_Count == 0)
                Debug.LogErrorFormat("No cards in pile {0}.", Context.PileNum);

            // Adjust Card Position
            if (Context.PileNum == 0 || Context.PileNum == 7)  // Card is in a Deck Pile
                card.transform.localPosition = new Vector3(0, (Pile_Count - (int)i - 1) * (GameManager.Instance.cardThickness), 0); // Stack by Card Height
            else if (Context.PileNum == 1 || Context.PileNum == 6)  // Card is in a Discard Pile
                card.transform.localPosition = new Vector3(0, (Pile_Count - (int)i - 1) * (GameManager.Instance.cardThickness), 0); // Stack by Card Height
            else if (Context.PileNum > 0) // Card is in a Table Pile
            {
                card.transform.localPosition = new Vector3(0, (Pile_Count - (int)i - 1) * (GameManager.Instance.cardThickness), (Pile_Count - (int)i - 1) * GameManager.Instance.pileOffset); // Stack by Card Height

                GameObject CardCollider = GameManager.Instance.getPileCardCollider(Context.PileNum);
                CardCollider.transform.localPosition = new Vector3(0, (Pile_Count - 1) * (GameManager.Instance.cardThickness), (Pile_Count - 1) * GameManager.Instance.pileOffset); // Adjust collider
            }
            else if (Context.PileNum == -1)  // Card is held in the dominant hand
                Debug.LogError("Pile State set while card is held in the dominant hand!");
            else if (Context.PileNum == -2)  // Card is held in the inferior hand
                Debug.LogError("Pile State set while card is held in the inferior hand!");

            // Set Card GO Rotation
            if (Context.FaceUp)
                card.transform.localRotation = Quaternion.Euler(0, 0, 0); // Rotate Face Up
            else
                card.transform.localRotation = Quaternion.Euler(180, 0, 0); // Rotate Face Down
        }

        /*
         *      Process an event raised by the Dominant Hand Collision Detector.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Notify()
        {
            if (!initialized)  // at least one logic update cycle must be performed before processing events
                return;

            if (!topCard)  // filter out all but the top most card in each stack
                return;

            if (Context.PileNum != DominantHandCollisionDetector.Instance.pileHit)  // filter out actions on other piles
                return;

            if (Context.PileNum == -3)  // Cannot pickup free cards. Do not claim ownership of free cards (initialization band-aid)
                return;

            int pile_num = DominantHandCollisionDetector.Instance.pileHit;

            GameManager.Instance.setDebugText("Pile Number: " + DominantHandCollisionDetector.Instance.pileHit.ToString());  // output debug text

            if (GameManager.Instance.StateDominantHand == GameManager.DominantHandState.Free)  // If hand is free, attempt to pick up the card
            {
                if (Time.time > Context.lock_timeout || pile_num != Context.lastPileNum)  // Avoid repetitive attempts to pick up cards from the same pile, but allow other piles to be picked up without delay
                {
                    if (GameManager.Instance.getNumCards(pile_num) > 0 && GameManager.Instance.getNumCardsHand() == 0)  // Perform Transfer if: 1) Pile has cards, and 2) The Hand is Empty.
                    {
                        // Update detection hyperparameters
                        Context.lock_timeout = Time.time + Context.timeout_sec;  // Prevent picking up another card too soon from the same pile
                        Context.lastPileNum = pile_num;  // Allow picking up another card from another pile immediately

                        // Get Card GameObject
                        GameObject card = Context.gameObject;  // To update the context transform information

                        // Update Debug
                        GameManager.Instance.setDebugPriorityText("Picked: " + card.name);  // output debug text

                        // Update GameObject
                        card.transform.parent = HandsManager.Instance.DominantFingerCardAnchor;  // attach card to the dominant finger anchor

                        // Update Context
                        Context.PileNum = -1;  // To Hand
                        Context.Position = card.transform.position;
                        Context.Rotation = card.transform.rotation;

                        // Update the Game Manager
                        GameManager.Instance.transferCardPile2Hand(pile_num);

                        // Play Audio
                        AudioManager.Instance.PlayCardSwipe();

                        // Change Owner to Current User
                        CardControlData data = new CardControlData(Context.CardID, PlayerManager.Instance.PlayerNum);
                        SendData command = new SendData(data: data, SendReliable: true, ReceiveLocally: true);
                        Invoker.Instance.SetCommand(command);
                        Invoker.Instance.ExecuteCommand(false);  // do not record command history

                        // Send to Remote clients
                        CardToPile data2 = new CardToPile(cardID: Context.CardID, pile: -3, name: Context.name);  // updates remote clients to transition from Pile State -> Move State
                        SendData command2 = new SendData(data: data2, SendReliable: true, ReceiveLocally: false);
                        Invoker.Instance.SetCommand(command2);
                        Invoker.Instance.ExecuteCommand(false);  // do not record command history

                        // Update State
                        Context.ChangeState(Context.fingerState);  // Pick up the card (Change State to FingerState)
                    }
                }
            }
        }
    }
}
