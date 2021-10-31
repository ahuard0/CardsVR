using CardsVR.Interaction;
using UnityEngine;
using CardsVR.Commands;

namespace CardsVR.States
{
    public class FingerState : BaseState, IObserver
    {
        private CardStateContext Context;
        private bool initialized;  // ensure at least one logic update has been performed before processing events

        public FingerState(CardStateContext stateMachine) : base("FingerState", stateMachine)
        {
            Context = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            DominantHandCollisionDetector.Instance.AttachObserver(this);

            initialized = false;  // at least one logic update cycle must be performed before processing Notify() events

            Context.PileNum = -1;
            Context.FaceUp = true;
        }

        public override void Exit()
        {
            base.Exit();
            DominantHandCollisionDetector.Instance.DetachObserver(this);

            Message msg = new Message(Context.gameObject.name);

            if (msg != null)
            {
                Invoker.Instance.SetCommand(new SendMessage(msg));
                Invoker.Instance.ExecuteCommand(false);  // do not record sends in command history
            }
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            initialized = true;  // at least one logic update cycle must be performed before processing Notify() events

            // Get the Card GO and Number of Cards in the Stack
            GameObject card = GameManager.Instance.getCardByTag(Context.CardID);
            if (card == null)
                return;

            int Pile_Count = GameManager.Instance.getNumCards(Context.PileNum);
            if (Pile_Count == 0)
                return;

            // Set the Card Pile Anchor (GameObject Parent)
            Transform cardAnchor = HandsManager.Instance.DominantFingerCardAnchor;
            if (cardAnchor == null)
                return;
            card.transform.parent = cardAnchor;

            // Set Card Position
            int? i = GameManager.Instance.getStackPosition(Context.CardID, Context.PileNum);  // Get the position within the stack
            if (i == null)
                Debug.LogError("Card not found in the pile stack!");

            // Position the Card in the Stack
            card.transform.localPosition = new Vector3((int)i * 0.025f, (int)i * GameManager.Instance.cardThickness, 0); // Stack by Card Height
            card.transform.localRotation = Quaternion.Euler(0, 0, 0); // Rotate Face Up
        }

        /*
         *      Movement of the cards is processed within the FixedUpdate() loop of the context GameObject, 
         *      which is implemented by this UpdatePhysics() method.
         *      
         *      During each fixed time frame, the current position and rotation are sent over the network
         *      to listening clients.  These clients syncronize the remote position of cards in the MoveState.
         *      FingerState handles the transmission of global coordinates to the remote clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            if (!initialized)
                return;

            // Send to Remote clients
            Movement movement = new Movement(Context.gameObject.name, Context.gameObject.transform.position, Context.gameObject.transform.rotation, Movement.Type.Absolute);
            SendData command = new SendData(movement, false, false);
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
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

            int pile_num = DominantHandCollisionDetector.Instance.pileHit;

            GameManager.Instance.setDebugText("Pile Number: " + DominantHandCollisionDetector.Instance.pileHit.ToString());  // output debug text

            if (GameManager.Instance.StateDominantHand == GameManager.DominantHandState.Held)  // If a card is held, attempt to place the card on a pile
            {
                if (Time.time > Context.lock_timeout || pile_num != Context.lastPileNum)  // Avoid repetitive attempts to place cards on the same pile, but allow other piles to be placed without delay
                {
                    if (GameManager.Instance.getNumCardsHand() > 0)  // Perform Transfer if a card is in the hand
                    {
                        // Update detection hyperparameters
                        Context.lock_timeout = Time.time + Context.timeout_sec;  // Prevent picking up another card too soon from the same pile
                        Context.lastPileNum = pile_num;  // Allow placement on another pile immediately

                        // Get Card GameObject
                        GameObject card = Context.gameObject;  // To update the context transform information

                        // Update Debug
                        GameManager.Instance.setDebugPriorityText("Placed: " + card.name);  // output debug text

                        // Update Context
                        Context.PileNum = pile_num;  // To pile
                        Context.Position = card.transform.position;
                        Context.Rotation = card.transform.rotation;

                        // Send to Remote clients
                        CardToPile data = new CardToPile(Context.CardID, Context.PileNum, Context.name);
                        SendData command = new SendData(data, true, true);
                        Invoker.Instance.SetCommand(command);
                        Invoker.Instance.ExecuteCommand(true);  // record command history

                        // Update State
                        Context.ChangeState(Context.pileState);  // Place the card (set state to PileState)
                    }
                }
            }
        }
    }
}
