using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   ConfigHandReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   ConfigHandReceiver is called by the Execute method of the ConfigHand class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class ConfigHandReceiver
    {
        private static HandsClient _client;

        /*
         *      Sets the Oculus user ID data of a remote Hand.
         *      
         *      Parameters
         *      ----------
         *      data : HandData
         *          A command object representing the pile model data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(HandData data)
        {
            if (_client == null)
                _client = (HandsClient)GameObject.FindObjectOfType(typeof(HandsClient));

            _client.SetHand(data);
        }
    }
}
