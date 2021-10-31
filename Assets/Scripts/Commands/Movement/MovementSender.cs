using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace CardsVR.Commands
{
    /*
     *   MovementSender follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   MovementSender is called by the Execute method of the SendMovement class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class MovementSender
    {
        /*
         *      Broadcast a movement object to all PUN clients.  Raising a PUN event triggers the OnEvent 
         *      PUN callback in MovementClient.
         *      
         *      The sender also receives the PUN object sent.  OnEvent is triggered for every player
         *      connected in a Room, including the player who sent the command.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          A movement object to be sent over the PUN network.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Broadcast(Movement movement)
        {
            if (movement == null)
                return;

            object[] data = movement.ToObjectArray();  // Photon can serialize each individual object for us (the preferred way)

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // Event will be received by the local client as well as remote clients.
            PhotonNetwork.RaiseEvent(Movement.EventID, data, raiseEventOptions, SendOptions.SendUnreliable);
        }
    }
}
