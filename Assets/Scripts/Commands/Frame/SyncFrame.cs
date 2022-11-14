using CardsVR.Detection;

namespace CardsVR.Commands
{
    /*
     *      A Command that sets the Frame model data.
     *      
     *      A SyncFrameReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A FrameData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the FrameData class structure.
     */
    public class SyncFrame: Command
    {
        public FrameData data;

        /*
         *      Constructor for the SyncFrame class.
         *      
         *      Parameters
         *      ----------
         *      command : FrameData
         *          The object representing the model model data.
         */
        public SyncFrame(FrameData data)
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
            SyncFrameReceiver.Receive(data);  // method defined by the SyncFrameReceiver (receiver class) in a Command design pattern
        }
    }
}
