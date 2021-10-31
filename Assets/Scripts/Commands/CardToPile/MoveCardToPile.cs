namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Data Sender to handle commands sent over PUN.
     *      
     *      A MoveCardToPile is a "receiver" type class following the Command design pattern.
     *      
     *      A CardToPile is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the CardToPile class structure.
     */
    public class MoveCardToPile: Command
    {
        public CardToPile data;

        /*
         *      Constructor for the MoveCardToPile class.
         *      
         *      Parameters
         *      ----------
         *      command : CardToPile
         *          The object representing the movement of a card to a pile.
         */
        public MoveCardToPile(CardToPile data)
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
            MoveCardToPileReceiver.Receive(data);  // method defined by the MoveCardToPileReceiver (receiver class) in a Command design pattern
        }
    }
}
