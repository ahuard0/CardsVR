using CardsVR.Interaction;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   SyncTableHeightReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   SyncTableHeightReceiver is called by the Execute method of the SyncTableHeight class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class SyncTableHeightReceiver
    {
        /*
         *      Sets the Table Height model data.
         *      
         *      Parameters
         *      ----------
         *      data : SyncTableHeight
         *          A command object representing the Table Height model data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(TableHeightData data)
        {
            GameObject GO = GameObject.Find("TableHeightClient");
            TableHeightHandsClient client = GO.GetComponent<TableHeightHandsClient>();
            client.ReceiveTableData(data);
        }
    }
}
