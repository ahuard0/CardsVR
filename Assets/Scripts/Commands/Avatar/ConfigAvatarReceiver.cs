using CardsVR.Interaction;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   ConfigAvatarReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   ConfigAvatarReceiver is called by the Execute method of the ConfigAvatar class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class ConfigAvatarReceiver
    {
        private static AvatarClient _client;

        /*
         *      Sets the Oculus user ID data of a remote Avatar.
         *      
         *      Parameters
         *      ----------
         *      data : AvatarData
         *          A command object representing the pile model data.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Receive(AvatarData data)
        {
            if (_client == null)
                _client = (AvatarClient)GameObject.FindObjectOfType(typeof(AvatarClient));

            _client.SetAvatar(data);
        }
    }
}
