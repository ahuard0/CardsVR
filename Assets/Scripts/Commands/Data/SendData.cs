using ExitGames.Client.Photon;
using Photon.Realtime;

namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Movement Sender to handle movements sent over PUN.
     *      
     *      A MovementSender is a "receiver" type class following the Command design pattern.
     *      
     *      A Movement is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the Movement class structure.
     */
    public class SendData : Command
    {
        public ICommandData data;
        public bool SendReliable = false;
        public bool ReceiveLocally = true;

        /*
         *      Constructor for the SendMovement class.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          The Movement object to be sent over the PUN network to all connected clients within a Room.
         */
        public SendData(ICommandData data)
        {
            this.data = data;
        }
        public SendData(ICommandData data, bool SendReliable)
        {
            this.data = data;
            this.SendReliable = SendReliable;
        }
        public SendData(ICommandData data, bool SendReliable, bool ReceiveLocally)
        {
            this.data = data;
            this.SendReliable = SendReliable;
            this.ReceiveLocally = ReceiveLocally;
        }

        /*
         *      Executes the command.  This is a required interface method for all commands.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void Execute()
        {
            if (SendReliable == false && ReceiveLocally == false)
                DataSender.Broadcast(data, SendOptions.SendUnreliable, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
            else if (SendReliable == true && ReceiveLocally == false)
                DataSender.Broadcast(data, SendOptions.SendReliable, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
            else if (SendReliable == false && ReceiveLocally == true)
                DataSender.Broadcast(data, SendOptions.SendUnreliable, new RaiseEventOptions { Receivers = ReceiverGroup.All });
            else if (SendReliable == true && ReceiveLocally == true)
                DataSender.Broadcast(data, SendOptions.SendReliable, new RaiseEventOptions { Receivers = ReceiverGroup.All });
        }
    }
}
