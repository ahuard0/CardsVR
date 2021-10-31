using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace CardsVR.Networking
{
    /*
     *      The lobby connection observer monitors the connection manager 
     *      for changes in connection state.  If the connection manager
     *      updates state, this observer is notified and the Lobby GUI
     *      refreshed.
     */

    public class LobbyConnectionObserver : MonoBehaviour, IObserver
    {
        private bool isLoading = false;

        [Header("Managers")]
        public ConnectionManager Connection;  // The CardsVR Connection Manager, which is referenced by this observer to retrieve the connection state.

        [Header("GUI Elements")]
        public Text ConnectionLabel;
        public Text LobbyStatusLabel;
        public Text RoomStatusLabel;
        public Text RoomsInLobbyLabel;

        [Header("Options")]
        public bool AutoLoadSceneOnJoinRoom = false;
        public string AutoLoadSceneName = "";

        /*
         *      When the gameObject is enabled, this observer is registered with the subject object iaw 
         *      the Observer design pattern.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnEnable()
        {
            Connection.AttachObserver(this);  // Attach this observer this to the connection manager subject
            Reset();
        }

        /*
         *      To avoid memory leaks, a potential issue with the Observer design pattern, hard references to 
         *      this observer are removed from the connection manager subject class whenever this object is
         *      disabled.  GameObject is disabled when changing scene or when the object is destroyed.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnDisable()
        {
            Connection.DetachObserver(this);  // Detach this observer this to the connection manager subject
        }

        /*
         *      The Notify callback updates the GUI with the current state of the connection manager.
         *      
         *      Optionally, the CardsVR scene is automatically loaded after joining a room if 
         *      AutoLoadSceneOnJoinRoom is true.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Notify()
        {
            UpdateMasterConnectStatus();
            UpdateLobbyStatus();
            UpdateRoomsInLobbyStatus();
            UpdateJoinedRoomStatus();
            AutoLoadSceneIfInRoom();
        }

        /*
         *      Reset all private members used by this class such as the isLoading parameter.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void Reset()
        {
            isLoading = false;
        }

        /*
         *      Automatically load the CardsVR scene if three conditions are met:
         *          1) User must be in a room
         *          2) The scene must not currently be loading
         *          3) The parameter AutoLoadSceneOnJoinRoom is true.
         *      
         *      The lobby scene is destroyed upon loading the CardsVR scene including
         *      all running gameObjects.  The Connection Manager singleton will persist 
         *      in the new scene after loading.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void AutoLoadSceneIfInRoom()
        {
            if (PhotonNetwork.InRoom && AutoLoadSceneOnJoinRoom && !isLoading && !AutoLoadSceneName.Equals(""))
            {
                isLoading = true;
                PhotonNetwork.LoadLevel(AutoLoadSceneName);
            }
        }

        /*
         *      Updates the text corresponding to the master connection status label.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void UpdateMasterConnectStatus()
        {
            if (Connection.master == ConnectionManager.ConnectState.connected)
            {
                ConnectionLabel.text = "Connected to Master as " + PhotonNetwork.LocalPlayer.NickName;
            }
            else
            {
                ConnectionLabel.text = "Disconnected from Master";
            }
        }

        /*
         *      Updates the text corresponding to the lobby status label.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void UpdateLobbyStatus()
        {
            if (PhotonNetwork.InLobby)
                LobbyStatusLabel.text = "In Lobby";
            else
                LobbyStatusLabel.text = "Not In Lobby";
        }

        /*
         *      Updates the text corresponding to the Joined Room status label.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void UpdateJoinedRoomStatus()
        {
            if (PhotonNetwork.InRoom)
            {
                RoomStatusLabel.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;
            }
            else
                RoomStatusLabel.text = "Not in a Room";
        }

        /*
         *      Updates the text corresponding to the list of rooms in lobby status label.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void UpdateRoomsInLobbyStatus()
        {
            if (Connection.cachedRoomList.Count > 0)
            {
                string roomListStr = "";
                int ndx = 0;
                foreach (RoomInfo info in Connection.cachedRoomList.Values)
                {
                    ndx += 1;
                    roomListStr += info.Name + ": " + info.PlayerCount.ToString() + " Players with " + info.MaxPlayers.ToString() + " Max Players";
                    if (ndx < Connection.cachedRoomList.Count)
                        roomListStr += ", ";
                }
                RoomsInLobbyLabel.text = roomListStr;
            }
            else
            {
                RoomsInLobbyLabel.text = "No Rooms in Lobby";
            }
        }
    }
}
