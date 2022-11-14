using CardsVR.Interaction;

namespace CardsVR.Commands
{
    /*
     *   RequestCardControlReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   RequestCardControl is called by the PileState and FingerState classes.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class RequestCardControlReceiver
    {
        /*
         *      Moves a card GameObject according to the instructions contained within the RequestCardControl command.
         *      
         *      Parameters
         *      ----------
         *      command : CardControlData
         *          A command object representing the change in state of a card.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(CardControlData command)
        {
            int[] CardID = command.CardID;
            int PlayerID = command.PlayerID;

            foreach(int card_ID in CardID)
                GameManager.Instance.setCardOwned(card_ID, PlayerID);
        }
    }
}
