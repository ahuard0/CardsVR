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
    public static class DataSender
    {
        /*
         *      Broadcast a data object to all PUN clients.  Raising a PUN event triggers the OnEvent 
         *      PUN callback in clients implementing IOnEventCallback.
         *      
         *      The sender also receives the PUN object sent.  OnEvent is triggered for every player
         *      connected in a Room, including the player who sent the command.
         *      
         *      Parameters
         *      ----------
         *      data : ICommandData
         *          A command data object to be sent over the PUN network.
         *      options : SendOptions (optional)
         *          PUN setting for sending data.  Can either be SendOptions.SendReliable or SendOptions.SendUnreliable.
         *          Default is SendOptions.Unreliable.
         *      raiseEventOptions : RaiseEventOptions (optional)
         *          PUN setting object determining the behavior of remote clients.  If using ReceiverGroup.All,
         *          all subscribers will receive the remote event, including the local client.
         *          Example:
         *              RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Broadcast(ICommandData data)
        {
            Broadcast(data, SendOptions.SendUnreliable);
        }
        public static void Broadcast(ICommandData data, SendOptions options)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // Event will be received by the local client as well as remote clients.
            Broadcast(data, options, raiseEventOptions);
        }
        public static void Broadcast(ICommandData data, SendOptions options, RaiseEventOptions raiseEventOptions)
        {
            if (data == null)
                return;
            
            object[] dataArray = data.ToObjectArray();  // Photon can serialize each individual object for us (the preferred way)

            PhotonNetwork.RaiseEvent(data.getEventID, dataArray, raiseEventOptions, options);
        }
    }
}
