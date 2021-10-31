using CardsVR.States;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardsVR.Commands
{
    /*
     *      MovementClient handles the sending and receiving of Movements, 
     *      registers callbacks, and acts as the nexus of the messaging 
     *      protocol.
     *      
     *      This MonoBehavior component should be attached to a GameObject
     *      in the scene.  
     *      
     *      IOnEventCallback is implemented to receive PUN Events using the
     *      OnEvent callback method.
     *      
     *      IMovementClient is implemented to provide the SetMovement callback
     *      method for MovementReceiver.
     */
    public class MovementClient : MonoBehaviour, IOnEventCallback, IMovementClient
    {
        public Invoker invoker;  // GameObject component that provides FixedUpdate timing for replaying commands. Also executes commands.

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
            MovementReceiver.client = this;
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
            MovementReceiver.client = null;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        /*
         *      The Move callback is required to interface with the MovementReceiver
         *      static class.  This callback receives events from MovementReceiver.
         *      
         *      MovementReceiver will use this callback to initiate movement of elements 
         *      in the scene.
         *      
         *      Movement will be handled by the MoveState component, which implements
         *      the position and rotation maintained by CardStateContext.  This state
         *      design pattern follows the model/view pattern where Context is the model
         *      and the state's MonoBehavior Update() loop is the view.
         *      
         *      This method is intended to be used with PUN to synchronize the position
         *      and rotation of objects in a scene.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          Defines the action needed to move an object.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Move(Movement movement)
        {
            GameObject go = GameObject.Find(movement.Name);
            if (go is null)
                return;

            CardStateContext context = go.GetComponent<CardStateContext>();
            if (context is null)
                return;

            // Movement is always commanded in Absolute coordinates
            if (movement.type == Movement.Type.Absolute)
            {
                context.Position = movement.Pos;
                context.Rotation = movement.Rot;
            }
            else if (movement.type == Movement.Type.Relative)
            {
                context.Position = go.transform.position + movement.Pos;
                context.Rotation = Quaternion.Euler(go.transform.rotation.eulerAngles + movement.Euler);
            }

            context.ChangeState(context.moveState);
        }

        /*
         *      The main loop for Physics events.
         *      
         *      This client detects user interaction and issuse the appropriate commands to keep objects in Sync across the PUN network.
         *      
         *      FixedUpdate is chosen to match the Invoker replay loop rate.  This makes command replay speed platform independent across systems.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void FixedUpdate()
        {
            if (!invoker.isReplaying)
            {

                Movement movement = null;
                if (Input.GetKey(KeyCode.Q))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(5, -5, 0), Movement.Type.Relative);
                else if (Input.GetKey(KeyCode.W))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(5, 0, 0), Movement.Type.Relative);
                else if (Input.GetKey(KeyCode.E))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(5, 5, 0), Movement.Type.Relative);
                else if (Input.GetKey(KeyCode.A))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(0, -5, 0), Movement.Type.Relative);
                else if (Input.GetKey(KeyCode.S))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(-5, 0, 0), Movement.Type.Relative);
                else if (Input.GetKey(KeyCode.D))
                    movement = new Movement("CardToMove", Vector3.zero, Vector3.zero + new Vector3(0, 5, 0), Movement.Type.Relative);


                //Transform t = GameObject.Find("CardToMove").transform;
                //Vector3 pos = t.position;
                //Vector3 rot = t.rotation.eulerAngles;
                //Movement movement = null;
                //if (Input.GetKey(KeyCode.Q))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(5, -5, 0), Movement.Type.Absolute);
                //else if (Input.GetKey(KeyCode.W))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(5, 0, 0), Movement.Type.Absolute);
                //else if (Input.GetKey(KeyCode.E))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(5, 5, 0), Movement.Type.Absolute);
                //else if (Input.GetKey(KeyCode.A))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(0, -5, 0), Movement.Type.Absolute);
                //else if (Input.GetKey(KeyCode.S))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(-5, 0, 0), Movement.Type.Absolute);
                //else if (Input.GetKey(KeyCode.D))
                //    movement = new Movement("CardToMove", pos, rot + new Vector3(0, 5, 0), Movement.Type.Absolute);

                if (movement != null)
                {
                    invoker.SetCommand(new SendMovement(movement));
                    invoker.ExecuteCommand(false);  // do not record sends in command history
                }
            }
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

            if (EventCode == Movement.EventID)
            {
                // Create Movement Object
                object[] data = (object[])photonEvent.CustomData;
                Movement msg = Movement.FromObjectArray(data);

                // Create Command
                ReceiveMovement command = new ReceiveMovement(msg);

                // Invoke Command
                invoker.SetCommand(command);
                invoker.ExecuteCommand(true);  // record command history
            }
        }
    }
}
