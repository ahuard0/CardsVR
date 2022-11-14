using CardsVR.Interaction;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   SyncDeckPosReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   SyncDeckPosReceiver is called by the Execute method of the SyncDeckPos class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class SyncDeckPosReceiver
    {
        /*
         *      Sets the pile position.
         *      
         *      Parameters
         *      ----------
         *      data : DeckPosData
         *          A command object representing the pile position data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(DeckPosData data)
        {
            GameObject pile = GameManager.Instance.getPile(data.PileID);
            Vector3 prev_pos = pile.transform.position;

            Vector3 new_pos = new Vector3(data.Position.x, prev_pos.y, data.Position.z);
            pile.transform.position = new_pos;
        }
    }
}
