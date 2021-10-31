using UnityEngine;

namespace CardsVR.Networking
{

    /*
     *      Attach this script to a gameObject in the scene to automatically connect
     *      to the master server, join the default lobby, get a list of rooms currently
     *      available, and either join the first room or create a room if none exist.
     *      
     *      Optionally, this script can automatically connect and join on launch.
     */
    public class LobbyAutoConnect : MonoBehaviour
    {
        [Header("Managers")]
        public ConnectionManager Connection;

        [Header("Options")]
        public bool autoConnectAndJoinOnEnable = false;

        /*
        *      OnEnable is Run when gameObject is enabled after its creation by Unity Engine.
        *      This method is called after Awake and Start are called in other gameObjects.
        *      
        *      Parameters
        *      ----------
        *      None
        *      
        *      Returns
        *      -------
        *      None
        */
        void OnEnable()
        {
            if (autoConnectAndJoinOnEnable)
                AutoConnectAndJoinRoom();
        }

        /*
        *      Automatically connect to the master server and Join/Create a room.
        *      
        *      A room is created if none currently exist.  If a room exists, attempt to join
        *      it.
        *      
        *      Parameters
        *      ----------
        *      None
        *      
        *      Returns
        *      -------
        *      None
        */
        public void AutoConnectAndJoinRoom()
        {
            if (Connection != null)
            {
                Connection.Connect(true);  // autoconnect mode triggered in Connection Manager
            }
        }
    }
}
