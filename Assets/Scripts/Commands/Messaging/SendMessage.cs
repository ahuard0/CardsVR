namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Message Sender to handle messages sent over PUN.
     *      
     *      A MessageSender is a "receiver" type class following the Command design pattern.
     *      
     *      A Message is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the Message class structure.
     */
    public class SendMessage: Command
    {
        public Message message;

        /*
         *      Constructor for the SendMessage class.
         *      
         *      Parameters
         *      ----------
         *      message : Message
         *          The Message object to be sent over the PUN network to all connected clients within a Room.
         */
        public SendMessage(Message message)
        {
            this.message = message;
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
            MessageSender.Broadcast(message);  // method defined by the MessageSender (receiver class) in a Command design pattern
        }
    }
}
