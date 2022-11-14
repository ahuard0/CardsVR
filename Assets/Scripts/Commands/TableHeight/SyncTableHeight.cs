namespace CardsVR.Commands
{
    /*
     *      A Command that sets the TableHeight model data.
     *      
     *      A SyncTableHeightReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A TableHeightData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the TableHeightData class structure.
     */
    public class SyncTableHeight: Command
    {
        public TableHeightData data;

        /*
         *      Constructor for the SyncTableHeight class.
         *      
         *      Parameters
         *      ----------
         *      command : TableHeightData
         *          The object representing the model data.
         */
        public SyncTableHeight(TableHeightData data)
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
            SyncTableHeightReceiver.Receive(data);  // method defined by the SyncTableHeightReceiver (receiver class) in a Command design pattern
        }
    }
}
