using System.Collections;
using System.Collections.Generic;

namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Data Sender to handle commands sent over PUN.
     *      
     *      A SyncCard is a "receiver" type class following the Command design pattern.
     */
    public class SyncCard: Command
    {
        public CardSync data;

        /*
         *      Constructor for the SyncCard class.
         *      
         *      Parameters
         *      ----------
         *      command : CardSync
         *          The object representing the movement of a card to a pile.
         */
        public SyncCard(CardSync data)
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
            SyncCardReceiver.Receive(data);  // method defined by the SyncCardReceiver (receiver class) in a Command design pattern
        }

        
    }
}
