namespace CardsVR.Commands
{
    /*
     *      The Command base class governs the interface contract for the Command design pattern.
     *      
     *      Command objects are aggregated by the Invoker and stored in the CommandRecorder
     *      Singleton static class.
     */
    public abstract class Command
    {
        /*
         *      The Execute command must be overridden to exercise the corresponding receiver.
         *      For example, RecieveMessage inherits this Command base class and overrides 
         *      the Execute method to interface with the MessageReceiver static class.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public abstract void Execute();
    }
}
