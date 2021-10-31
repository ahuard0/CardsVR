using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace CardsVR.Networking
{
    /*
     *      A utility class that monitors the Photon network 
     *      connection and updates a text box with the connection 
     *      status.
     */
    public class ConnectionMonitor : MonoBehaviour
    {
        [Header("UI References")]
        public Text ConnectionStatusText;
        public Text RoomStatusText;
        public GameObject RoomStatusPanel;

        /*
        *      Update is run on every frame by Unity Engine.
        *      
        *      Parameters
        *      ----------
        *      None
        *      
        *      Returns
        *      -------
        *      None
        */
        private void Update()
        {
            UpdateStatus();
        }

        /*
        *      UpdateStatus updates the status GUI elements of the Network Gym/Test Bench scene.
        *      
        *      This 
        *      
        *      Parameters
        *      ----------
        *      None
        *      
        *      Returns
        *      -------
        *      None
        */
        private void UpdateStatus()
        {
            if (ConnectionStatusText != null)
                ConnectionStatusText.text = "Connection Status: " + PhotonNetwork.NetworkClientState + " (Room Count: " + PhotonNetwork.CountOfRooms.ToString() + ", Players: " + PhotonNetwork.CountOfPlayers + ")";

            if (RoomStatusPanel != null)
            {
                if (PhotonNetwork.InRoom)
                {
                    RoomStatusPanel.SetActive(true);
                    if (RoomStatusText != null)
                        RoomStatusText.text = "Room Status: In " + PhotonNetwork.CurrentRoom.Name + " (Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + ")";
                }

                else
                    RoomStatusPanel.SetActive(false);
            }
        }
    }
}
