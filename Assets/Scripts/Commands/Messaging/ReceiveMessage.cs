namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Message Receiver to handle received messages over PUN.
     *      
     *      A MessageReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A Message is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the Message class structure.
     */
    public class ReceiveMessage: Command
    {
        public Message message;

        /*
         *      Constructor for the ReceiveMessage class.
         *      
         *      Parameters
         *      ----------
         *      message : Message
         *          The Message object received within the OnEvent PUN callback in MessageClient.
         */
        public ReceiveMessage(Message message)
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
            MessageReceiver.DisplayMessage(message);  // method defined by the MessageReceiver (receiver class) in a Command design pattern
        }
    }
}
