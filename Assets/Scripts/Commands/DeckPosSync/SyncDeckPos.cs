namespace CardsVR.Commands
{
    /*
     *      A Command that sets the pile model data.
     *      
     *      A SyncDeckPosReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A DeckPosData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the DeckPosData class structure.
     */
    public class SyncDeckPos: Command
    {
        public DeckPosData data;

        /*
         *      Constructor for the SyncDeckPos class.
         *      
         *      Parameters
         *      ----------
         *      command : DeckPosData
         *          The object representing the model model data.
         */
        public SyncDeckPos(DeckPosData data)
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
            SyncDeckPosReceiver.Receive(data);  // method defined by the SyncDeckPosReceiver (receiver class) in a Command design pattern
        }
    }
}
