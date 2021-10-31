using CardsVR.Interaction;
using System.Collections.Generic;

namespace CardsVR.Commands
{
    /*
     *   SyncPileReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   SyncPileReceiver is called by the Execute method of the SyncPile class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class SyncPileReceiver
    {
        /*
         *      Sets the pile model data.
         *      
         *      Parameters
         *      ----------
         *      data : SyncPile
         *          A command object representing the pile model data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(PileData data)
        {
            GameManager.Instance.setPileCardStack(data.PileID, data.CardIDs);
        }
    }
}
