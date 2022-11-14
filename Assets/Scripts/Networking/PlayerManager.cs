using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CardsVR.Networking
{
    /*
     *  The PlayerManager monitors the ConnectionManager for changes in connection state, 
     *  which happen often at every PUN event in ConnectionManager.  Each time the connection
     *  changes state, this PlayerManager updates the cache of the player info.
     */
    public class PlayerManager : Singleton<PlayerManager>
    {

        [Header("Managers")]
        public ConnectionManager Connection;

        [Header("Options")]
        [SerializeField]
        private bool Spectator = false;

        [HideInInspector]
        public Player player;
        [HideInInspector]
        public Player[] playerListOthers;

        private string roomName;

        /*
         *      Specifies the player ID number.
         *      
         *      Parameters
         *      ----------
         *      int : PlayerNum
         *          An integer specifying the player ID (e.g., 1 or 2).
         */
        public int PlayerNum { get; set; }

        public int[] PlayerNumOthers 
        { 
            get
            {
                if (playerListOthers == null)
                    return null;

                List<int> Others = new List<int>();
                foreach (Player other in playerListOthers)
                    if (other.NickName.Contains("Player1") && !Others.Contains(1))
                        Others.Add(1);
                    else if (other.NickName.Contains("Player2") && !Others.Contains(2))
                        Others.Add(2);
                    else if (other.NickName.Contains("Player3") && !Others.Contains(3))
                        Others.Add(3);
                    else if (other.NickName.Contains("Player4") && !Others.Contains(4))
                        Others.Add(4);
                return Others.ToArray();
            }
        }

        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the PlayerManager to receive notifications from
         *      the ConnectionManager.
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
            StartCoroutine("CachePlayerInfo");
            StartCoroutine("AssignPlayerNum");
            StartCoroutine("ReconnectToServer");
            StartCoroutine("ReconnectToRoom");
        }

        /*
         *      Unity MonoBehavior callback OnDisable is called whenever the attached 
         *      GameObject is disabled.  This occurs when quitting the program or
         *      loading another scene.
         *      
         *      This method unregisters the PlayerManager from the ConnectionManager.
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
            StopCoroutine("CachePlayerInfo");
            StopCoroutine("AssignPlayerNum");
            StopCoroutine("ReconnectToServer");
            StopCoroutine("ReconnectToRoom");
        }

        /*
         *      A coroutine used to assign the player ID.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator AssignPlayerNum()
        {
            while(true)
            {
                if (PhotonNetwork.InRoom)
                    AssignPlayer();
                yield return new WaitForSecondsRealtime(0.5f);
            }
            
        }

        /*
         *      Assign the player ID to the current player based on
         *      the other players in the room.  This function will attempt to assign the player ID
         *      twice every second until it is successful.
         *      
         *      Reads the cached player information and assigns players seats at the table,
         *      which correspond to ID numbers 1-2.
         *      
         *      Players' headsets have the nickname suffix "A" while the external camera
         *      has the suffix "B".
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void AssignPlayer()
        {
            if (PlayerNum != 0)  // check nickname integrity if the player number is already assigned.
            {
                if (PlayerNum == 1 && !player.NickName.Contains("Player 1"))
                    player.NickName = "Player 1A";
                else if (PlayerNum == 2 && !player.NickName.Contains("Player 2"))
                    player.NickName = "Player 2A";
                else if (PlayerNum == -1 && !player.NickName.Contains("Spectator"))
                    player.NickName = "Spectator";
                return;
            }

            if (Spectator)
            {
                PhotonNetwork.NickName = "Spectator";
                player.NickName = PhotonNetwork.NickName;
                PlayerNum = -1;
                return;
            }

            if (playerListOthers == null)  // wait for the player ID number to be assigned.
                return;

            bool player1A = false;
            bool player2A = false;
            bool player1B = false;
            bool player2B = false;
            foreach (Player others in playerListOthers)
            {
                if (others.NickName.Contains("Player 1A"))
                    player1A = true;
                if (others.NickName.Contains("Player 2A"))
                    player2A = true;
                if (others.NickName.Contains("Player 1B"))
                    player1B = true;
                if (others.NickName.Contains("Player 2B"))
                    player2B = true;
            }
            if (player1B)  // Phone already joined
            {
                if (!player1A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 1A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 1;
                    return;
                }
                else if (!player2A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 2A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 2;
                    return;
                }
            }
            else if (player2B)  // Phone already joined
            {
                if (!player2A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 2A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 2;
                    return;
                }
                else if (!player1A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 1A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 1;
                    return;
                }
            }
            else
            {
                if (!player1A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 1A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 1;
                    return;
                }
                else if (!player2A)  // Same player headset not yet joined -> Join
                {
                    PhotonNetwork.NickName = "Player 2A";
                    player.NickName = PhotonNetwork.NickName;
                    PlayerNum = 2;
                    return;
                }
            }
        }

        /*
         *      Caches a reference to the player and other players.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator CachePlayerInfo()
        {
            while(true)
            {
                if (PhotonNetwork.InRoom)
                {
                    playerListOthers = PhotonNetwork.PlayerListOthers;
                    player = PhotonNetwork.LocalPlayer;
                    roomName = PhotonNetwork.CurrentRoom.Name;
                }
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        /*
         *      Attempts to reconnect to the master server.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator ReconnectToServer()
        {
            while (true)
            {
                if (!PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState != ClientState.ConnectingToMasterServer && PlayerNum != 0)
                {
                    PhotonNetwork.Reconnect();
                }
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        /*
         *      Attempts to rejoin the room.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator ReconnectToRoom()
        {
            while (true)
            {
                if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState != ClientState.Joining && !PhotonNetwork.InRoom && PlayerNum != 0)
                {
                    PhotonNetwork.JoinRoom(roomName);
                }
                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }
}
