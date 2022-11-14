namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Data Sender to handle commands sent over PUN.
     *      
     *      A RequestCard is a "receiver" type class following the Command design pattern.
     */
    public class RequestCard: Command
    {
        public RequestCardData data;

        /*
         *      Constructor for the RequestCard class.
         *      
         *      Parameters
         *      ----------
         *      command : CardSync
         *          The object representing the movement of a card to a pile.
         */
        public RequestCard(RequestCardData data)
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
            RequestCardReceiver.Receive(data);  // method defined by the RequestCardReceiver (receiver class) in a Command design pattern
        }
    }
}
