namespace CardsVR.Commands
{
    /*
     *      A Command that sets the pile model data.
     *      
     *      A SyncPileReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A PileData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the PileData class structure.
     */
    public class SyncPile: Command
    {
        public PileData data;

        /*
         *      Constructor for the SyncPile class.
         *      
         *      Parameters
         *      ----------
         *      command : PileData
         *          The object representing the model model data.
         */
        public SyncPile(PileData data)
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
            SyncPileReceiver.Receive(data);  // method defined by the SyncPileReceiver (receiver class) in a Command design pattern
        }
    }
}
