namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Data Sender to handle commands sent over PUN.
     *      
     *      A RequestCardControl is a "receiver" type class following the Command design pattern.
     */
    public class RequestCardControl: Command
    {
        public CardControlData data;

        /*
         *      Constructor for the RequestCardControl class.
         *      
         *      Parameters
         *      ----------
         *      command : CardControlData
         *          The object representing the card.
         */
        public RequestCardControl(CardControlData data)
        {
            this.data = data;
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
            RequestCardControlReceiver.Receive(data);  // method defined by the RequestCardControlReceiver (receiver class) in a Command design pattern
        }
    }
}
