using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardsVR.Networking
{
    /*
     *      The Connection Manager uses the Singleton Design Pattern, 
     *      which ensures that only one GameObject will be a ConnectionManager.  
     *      The gameObject this component attaches to will persist across scenes 
     *      and is not destroyed on loading of new scenes.
     *      
     *      This instance is also the subject of the observer design pattern.
     *      Observers are gameObjects that register themselves with this Subject 
     *      instance.  Observers are notified whenever a connection state change
     *      occurs that the GUI must respond to.  For example, when the lobby 
     *      scene is loaded, a GameObject containing the LobbyConnectionObserver
     *      will register this ConnectionManager and handle all GUI element
     *      manipulation.  This separates the ConnectionManager from GUI related
     *      tasks and adheres to the model/view design pattern.
     */

    public class ConnectionManager : SingletonPUN<ConnectionManager>, ISubject
    {

        public enum ConnectState { disconnected, connected };
        public ConnectState master = ConnectState.disconnected;
        private ArrayList _observers = new ArrayList();
        private bool AutoConnect = false;

        public Dictionary<string, RoomInfo> cachedRoomList;

        #region UNITY

        /*
         *      Awake is run before Start and initializes the Connection Manager.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void Awake()
        {
            base.Awake();
            PhotonNetwork.AutomaticallySyncScene = true;
            cachedRoomList = new Dictionary<string, RoomInfo>();
        }

        /*
         *      Disconnect from the server before application exit.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void OnApplicationQuit()
        {
            Disconnect();
        }

        /*
         *      Entry point into the networking routine.  The GUI will call this  
         *      function after an event such as a mouse click on a "Login" button.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */

        #endregion

        #region CALLBACKS

        /*
         *      Connect to the master server.  Set the region to US.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Connect(bool auto)
        {
            AutoConnect = auto;
            PhotonNetwork.LocalPlayer.NickName = "Player" + UnityEngine.Random.Range(1000, 9999);
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.ConnectToRegion("us");
        }

        /*
         *      Disconnect from the server.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Disconnect()
        {
            PhotonNetwork.Disconnect();
            master = ConnectState.disconnected;
            NotifyObservers();
        }

        /*
         *      Create a room on the master server.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void CreateRoom()
        {
            string roomName = "Room " + UnityEngine.Random.Range(1000, 10000);
            byte maxPlayers = 2;
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        /*
         *      Leave the current room.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        /*
         *      Join the default lobby.  This function also retrieves the list of 
         *      rooms and stores them in the cachedRoomList dictionary.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void JoinLobby()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
        }

        /*
         *      Leave the current lobby and go back to the master server state.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void LeaveLobby()
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }
        }

        /*
         *      Join a Room.
         *      
         *      Parameters
         *      ----------
         *      roomName : string
         *          The name of the room to join.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        /*
         *      Join a Room by index.  This is a utility function that is not intended 
         *      to be used for production.  It allows a script to quickly connect to the 
         *      first room available without needing to select a room from the list.
         *      
         *      Parameters
         *      ----------
         *      index : int
         *          The numeric index of the room to join in the cache dictionary.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void JoinRoom(int index)
        {
            if (cachedRoomList.Count > 0 && cachedRoomList.Count > index)
            {
                string[] keys = cachedRoomList.Keys.ToArray();
                JoinRoom(keys[index]);
            }
            else
                CreateRoom();
        }

        #endregion

        #region PUN

        /*
         *      PUN callback when connected to master server.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnConnectedToMaster()
        {
            master = ConnectState.connected;
            NotifyObservers();

            if (AutoConnect)
                JoinLobby();
        }

        /*
         *      Callback when the room list updates.  PUN updates the room list automatically while connected to a lobby.
         *      
         *      Automatically create or join a room if the Connection Manager's autoConnect flag was set true in the 
         *      Connect(auto) method.
         *      
         *      Parameters
         *      ----------
         *      roomList : List<RoomInfo>
         *          A list of rooms currently in the lobby.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            cachedRoomList.Clear();  // clear the room list
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
            NotifyObservers();

            if (AutoConnect)  // Autoconnect Flag is set by Connect(auto)
            {
                if (cachedRoomList.Count > 0)
                    JoinRoom(0);
                else
                    CreateRoom();

                AutoConnect = false;  // Reset Flag
            }
        }

        /*
         *      Callback when a lobby is joined.
         *      
         *      When Joining a Lobby, two callbacks result:
         *          1) OnJoinedLobby() is called first, followed by
         *          2) OnRoomListUpdate, which is dynamically called while connected to a lobby
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnJoinedLobby()
        {
            NotifyObservers();
        }

        /*
         *      Callback when leaving a lobby.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnLeftLobby()
        {
            NotifyObservers();
        }

        /*
         *      Callback when room creation fails.  Return to the lobby to select or create another room.
         *      
         *      Parameters
         *      ----------
         *      returnCode : short
         *          An error code.
         *          
         *      message : string
         *          An error message string.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            JoinLobby();  // Join the lobby to select another room to join
            NotifyObservers();
        }

        /*
         *      Callback when joining a random room fails.  Return to the lobby to select or create another room.
         *      
         *      Parameters
         *      ----------
         *      returnCode : short
         *          An error code.
         *          
         *      message : string
         *          An error message string.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            JoinLobby();  // Join the lobby to select another room to join
            NotifyObservers();
        }

        /*
         *      Callback when joining a room.  Notify the connection observer to update the GUI or take action such as loading the game scene.
         *      
         *      Sets the Player Nickname. (e.g., Player 1, Player 2, etc.)
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnJoinedRoom()
        {
            bool player1 = false;
            bool player2 = false;
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.NickName.Contains("Player 1"))
                    player1 = true;
                if (player.NickName.Contains("Player 2"))
                    player2 = true;
            }
            if (!player1)
            {
                PhotonNetwork.NickName = "Player 1";
                PlayerManager.Instance.PlayerNum = 1;
            }
            else if (!player2)
            {
                PhotonNetwork.NickName = "Player 2";
                PlayerManager.Instance.PlayerNum = 2;
            }
            else
            {
                Debug.LogError("Player 1 and 2 already connected!");
            }
            Debug.Log("Joined Room as " + PhotonNetwork.NickName);

            NotifyObservers();
        }

        /*
         *      Callback when leaving a room.  Notify the connection observer to update the GUI or take action such as returning to the lobby scene.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnLeftRoom()
        {
            NotifyObservers();
        }

        /*
         *      Callback when a player enters a room.  Notify the connection observer to update the GUI or take action such as updating a player list and initializing the new player.
         *      
         *      Parameters
         *      ----------
         *      newPlayer : Player
         *          The PUN player object for the new player.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            NotifyObservers();
        }

        /*
         *      Callback when a player leaves a room.  Notify the connection observer to update the GUI or take action such as updating a player list and removing the old player.
         *      
         *      Parameters
         *      ----------
         *      otherPlayer : Player
         *          The PUN player object for the old player.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            NotifyObservers();
        }

        /*
         *      Callback when a player leaves a room who was the master client.  Notify the connection observer to update the GUI or take action.
         *      
         *      Parameters
         *      ----------
         *      newMasterClient : Player
         *          The PUN player object for the new master client player.
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            NotifyObservers();
        }

        #endregion

        #region Observer

        /*
         *      Register an observer to be notified when changes occur to the Connection Manager.
         *      
         *      Parameters
         *      ----------
         *      IObserver observer : Observer object to register.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void AttachObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        /*
         *      Remove an observer from the Connection Manager's notification list.
         *      
         *      Parameters
         *      ----------
         *      IObserver observer : Observer object to register.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void DetachObserver(IObserver observer)
        {
            _observers.Remove(observer);
        }

        /*
         *      Notify all observers by triggering their Notify callback function.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void NotifyObservers()
        {
            foreach (IObserver observer in _observers)
                observer.Notify();
        }

        #endregion
    }
}
