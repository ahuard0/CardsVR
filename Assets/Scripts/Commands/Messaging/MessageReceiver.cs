using CardsVR.Interaction;

namespace CardsVR.Commands
{
    /*
     *   MessageReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   MessageReceiver is called by the Execute method of the ReceiveMessage class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class MessageReceiver 
    {
        /*
         *      Performs a callback to the SetMessage() callback registered by MessageClient.
         *      SetMessage() is a required method of the corresponding interface IMessageClient.
         *      
         *      Parameters
         *      ----------
         *      message : Message
         *          A message object received over the PUN network.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void DisplayMessage(Message message)
        {
            GameManager.Instance.setDebugPriorityText(message.Text);
        }
    }
}
