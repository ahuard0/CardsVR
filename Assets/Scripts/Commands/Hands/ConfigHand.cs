namespace CardsVR.Commands
{
    /*
     *      A Command that sets the Oculus UserID data for a remote Hand.
     *      
     *      A ConfigHandReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      An HandData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the HandData class structure.
     */
    public class ConfigHand : Command
    {
        public HandData data;

        /*
         *      Constructor for the ConfigHand class.
         *      
         *      Parameters
         *      ----------
         *      data : HandData
         *          The object representing the Hand data.
         */
        public ConfigHand(HandData data)
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
            ConfigHandReceiver.Receive(data);  // method defined by the ConfigHandReceiver (receiver class) in a Command design pattern
        }
    }
}
