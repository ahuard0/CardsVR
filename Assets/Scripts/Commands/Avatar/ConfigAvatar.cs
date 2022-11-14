namespace CardsVR.Commands
{
    /*
     *      A Command that sets the Oculus UserID data for a remote avatar.
     *      
     *      A ConfigAvatarReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      An AvatarData is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the AvatarData class structure.
     */
    public class ConfigAvatar : Command
    {
        public AvatarData data;

        /*
         *      Constructor for the ConfigAvatar class.
         *      
         *      Parameters
         *      ----------
         *      data : AvatarData
         *          The object representing the Avatar data.
         */
        public ConfigAvatar(AvatarData data)
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
            ConfigAvatarReceiver.Receive(data);  // method defined by the ConfigAvatarReceiver (receiver class) in a Command design pattern
        }
    }
}
