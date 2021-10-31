using CardsVR.Interaction;
using CardsVR.Commands;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace CardsVR.States
{
    /*
     *      Movement will be handled by the MoveState component, which implements
     *      the position and rotation maintained by CardStateContext.  This state
     *      design pattern follows the model/view pattern where Context is the model
     *      and the state's MonoBehavior Update() loop is the view.
     *      
     *      Assign this context to each Card capable of movement.
     */
    public class CardStateContext : StateMachine, IOnEventCallback
    {
        [HideInInspector]
        public PileState pileState { get; set; }
        [HideInInspector]
        public MoveState moveState { get; set; }
        [HideInInspector]
        public FingerState fingerState { get; set; }
        [HideInInspector]
        public bool FaceUp { get; set; }

        [Header("Data")]
        public Vector3 Position;
        public Quaternion Rotation;
        public int PileNum;
        public int CardID;

        [Header("Options")]
        public int lastPileNum;
        public float lock_timeout = 0f;
        public float timeout_sec = 2f;

        protected override BaseState GetInitialState()
        {
            return pileState;
        }

        private void Awake()
        {
            pileState = new PileState(this);
            moveState = new MoveState(this);
            fingerState = new FingerState(this);
        }

        private void Start()
        {
            Position = gameObject.transform.position;
            Rotation = gameObject.transform.rotation;
        }

        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the MovementClient with the Movement Reciever and
         *      registers the MovementClient to receive PUN events.
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
         *      This method unregisters the MovementClient with the Movement Reciever and
         *      unregisters the MovementClient from receiving PUN events.
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
         *      PUN uses this callback method to respond to PUN events. MovementClient must first be 
         *      registered to receive PUN events.
         *      
         *      MovementClient receives Movement data from players on PUN, including ourself, using
         *      the OnEvent callback.  For example, invoking the SendMovement command will trigger
         *      the OnEvent callback, which triggers a ReceiveMovement command for all players
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

            if (EventCode == Movement.EventID)  // process movement network events
            {
                // Create Movement Object
                object[] data = (object[])photonEvent.CustomData;
                Movement msg = Movement.FromObjectArray(data);

                if (msg.Name == gameObject.name)
                {
                    Position = msg.Pos;
                    Rotation = msg.Rot;

                    if (this.currentState == this.pileState)
                        ChangeState(this.moveState);
                }

                //// Create Command
                //Movement.ReceiveMovement command = new Movement.ReceiveMovement(msg);

                //// Invoke Command
                //invoker.SetCommand(command);
                //invoker.ExecuteCommand(true);  // record command history
            } else if (EventCode == Message.EventID)
            {
                // Create Message Object
                object[] data = (object[])photonEvent.CustomData;
                Message msg = Message.FromObjectArray(data);

                if (msg.Text == gameObject.name)
                {
                    if (this.currentState == this.moveState)
                        ChangeState(this.pileState);  // TODO:  Create a custom message class for this event.  Currently no pile is returned, and the code will break here.
                }
            }
        }
    }
}
