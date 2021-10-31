using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

namespace CardsVR.Networking
{
    public class PlayerManager : Singleton<PlayerManager>, IObserver
    {

        [Header("Managers")]
        public ConnectionManager Connection;

        public Player player;
        public Player[] playerListOthers;

        public int PlayerNum
        {
            get; set;
        }

        private void OnEnable()
        {
            Connection.AttachObserver(this);  // Attach this observer this to the connection manager subject
        }

        private void OnDisable()
        {
            Connection.DetachObserver(this);  // Detach this observer this to the connection manager subject
        }

        public void Notify()
        {
            CachePlayerInfo();
        }

        private void CachePlayerInfo()
        {
            if (PhotonNetwork.InRoom)
            {
                playerListOthers = PhotonNetwork.PlayerListOthers;
                player = PhotonNetwork.LocalPlayer;
            }  
        }
    }
}

