using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace CardsVR.Commands
{
    /*
     *   MessageSender follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   MessageSender is called by the Execute method of the SendMessage class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class MessageSender
    {
        /*
         *      Broadcast a message object to all PUN clients.  Raising a PUN event triggers the OnEvent 
         *      PUN callback in MessageClient.
         *      
         *      The sender also receives the PUN object sent.  OnEvent is triggered for every player
         *      connected in a Room, including the player who sent the command.
         *      
         *      Parameters
         *      ----------
         *      message : Message
         *          A message object to be sent over the PUN network.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Broadcast(Message message)
        {
            if (message == null)
                return;

            object[] data = message.ToObjectArray();  // Photon can serialize each individual object for us (the preferred way)

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // Event wll be received by the local client as well as remote clients.
            PhotonNetwork.RaiseEvent(Message.EventID, data, raiseEventOptions, SendOptions.SendUnreliable);
        }
    }
}
