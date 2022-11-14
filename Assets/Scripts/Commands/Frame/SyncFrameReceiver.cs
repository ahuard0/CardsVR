using CardsVR.Detection;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   SyncFrameReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   SyncFrameReceiver is called by the Execute method of the SyncFrame class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class SyncFrameReceiver
    {
        /*
         *      Sets the Frame model data.
         *      
         *      Parameters
         *      ----------
         *      data : SyncFrame
         *          A command object representing the Frame model data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(FrameData data)
        {
            Debug.Log(data.ToString());
            ArucoClient.Instance.Data = data;
        }
    }
}
