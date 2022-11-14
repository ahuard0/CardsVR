using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace CardsVR.Networking
{
    public class PlayerMonitor : Singleton<PlayerMonitor>
    {
        [Header("UI References")]
        public Text PlayerStatusText;
        public GameObject PlayerStatusPanel;

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
        }

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
        private void OnGUI()
        {
            UpdateStatus();
        }

        /*
        *      Update the player status panel text label.
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
            if (PlayerStatusPanel != null)
            {
                if (PhotonNetwork.InRoom)
                {
                    PlayerStatusPanel.SetActive(true);

                    if (PlayerStatusText != null)
                    {
                        if (PlayerManager.Instance.player != null)
                            PlayerStatusText.text = "Current Player (#" + PlayerManager.Instance.PlayerNum + "): " + PlayerManager.Instance.player.NickName + "\n";
                        else
                            PlayerStatusText.text = "No Player Instance" + "\n";
                        if (PlayerManager.Instance.playerListOthers != null)
                            foreach (Player player in PlayerManager.Instance.playerListOthers)
                                PlayerStatusText.text += "Other Player: " + player.NickName + "\n";
                    }
                }

                else
                    PlayerStatusPanel.SetActive(false);
            }
        }
    }
}

