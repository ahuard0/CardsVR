using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *      This is the interface contract of required functions for the MovementClient class.
     *      
     *      The required function, Move, is used by the MovementReceiver class to
     *      execute desired performance on the received Movement object.  MovementClient
     *      is attached to a Unity GameObject and contains references to GUI elements
     *      that Move() can operate on.
     */
    public interface IMovementClient
    {
        /*
         *      Interface for setting a message.  Implemented by MovementClient, which is
         *      a MonoBehavior class attached to a GameObject within the scene.
         *      
         *      Parameters
         *      ----------
         *      Name : string
         *          Name of the GameObject being moved.
         *      Pos : Vector3
         *          Translation vector relative to the origin.
         *      Rot : Vector3
         *          Euler angle rotation vector.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Move(Movement movement);
    }
}
