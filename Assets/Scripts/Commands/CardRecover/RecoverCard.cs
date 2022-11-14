namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Data Sender to handle commands sent over PUN.
     *      
     *      A RecoverCard is a "receiver" type class following the Command design pattern.
     */
    public class RecoverCard: Command
    {
        public CardRecover data;

        /*
         *      Constructor for the RecoverCard class.
         *      
         *      Parameters
         *      ----------
         *      command : CardSync
         *          The object representing the movement of a card to a pile.
         */
        public RecoverCard(CardRecover data)
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
            RecoverCardReceiver.Receive(data);  // method defined by the RecoverCardReceiver (receiver class) in a Command design pattern
        }
    }
}
