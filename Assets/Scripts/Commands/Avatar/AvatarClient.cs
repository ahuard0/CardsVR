using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using CardsVR.Networking;
using Oculus.Avatar;
using System;
using UnityEngine.SceneManagement;

namespace CardsVR.Commands
{
    public class AvatarClient : MonoBehaviour
    {
        private bool _initialized = false;
        private int PacketSequence = 0;
        private OvrAvatar LocalAvatar;

        [Header("OVR Local Avatars")]
        public OvrAvatar LocalAvatar1;
        public OvrAvatar LocalAvatar2;
        public OvrAvatar LocalAvatar3;
        public OvrAvatar LocalAvatar4;

        [Header("OVR Remote Avatars")]
        public OvrAvatar RemoteAvatar1;
        public OvrAvatar RemoteAvatar2;
        public OvrAvatar RemoteAvatar3;
        public OvrAvatar RemoteAvatar4;

        private bool OVRReady { get { return OvrAvatarSDKManager.Instance != null; } }

        /*
         *      Unity MonoBehavior callback OnEnable is called whenever the attached 
         *      GameObject is enabled.  On scene load, this occurs after Awake and 
         *      Start MonoBehavior callbacks.
         *      
         *      This method registers the client to receive PUN events.
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
            PhotonNetwork.AddCallbackTarget(this);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /*
         *      Unity MonoBehavior callback OnDisable is called whenever the attached 
         *      GameObject is disabled.  This occurs when quitting the program or
         *      loading another scene.
         *      
         *      This method unregisters the client from receiving PUN events.
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
            PhotonNetwork.RemoveCallbackTarget(this);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _initialized = true;
        }

        /*
         *      Unity MonoBehavior Callback used here to initialize the remote avatars 
         *      of the local player.
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
            if (OVRReady && _initialized && LocalAvatar == null)
            {
                // Select and Enable the Remote Avatar
                int playerID = PlayerManager.Instance.PlayerNum;
                if (playerID == 1)
                    LocalAvatar = LocalAvatar1;
                else if (playerID == 2)
                    LocalAvatar = LocalAvatar2;
                else if (playerID == 3)
                    LocalAvatar = LocalAvatar3;
                else if (playerID == 4)
                    LocalAvatar = LocalAvatar4;
                else
                    return;
                LocalAvatar.gameObject.SetActive(true);

                // Set Callback
                LocalAvatar.RecordPackets = true;
                LocalAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
            }
        }

        void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
        {
            byte PlayerID = (byte)PlayerManager.Instance.PlayerNum;
            uint size = CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
            byte[] data = new byte[size];
            CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, data);

            AvatarData msg = new AvatarData(data, size, PlayerID);
            SendData command = new SendData(data: msg, SendReliable: false, ReceiveLocally: false);
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
        }

        /*
         *      Method called by ConfigAvatarReceiver to set the Oculus UserID of remote avatars.
         *      
         *      Parameters
         *      ----------
         *      data : AvatarData
         *          The data object representing information needed to initialize a remote avatar.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void SetAvatar(AvatarData data)
        {
            if (!OVRReady)
                return;

            // Select and Enable the Remote Avatar
            byte playerID = data.PlayerID;
            OvrAvatar remote;
            if (playerID == 1)
                remote = RemoteAvatar1;
            else if (playerID == 2)
                remote = RemoteAvatar2;
            else if (playerID == 3)
                remote = RemoteAvatar3;
            else if (playerID == 4)
                remote = RemoteAvatar4;
            else
                return;
            if (!remote.gameObject.activeInHierarchy)
                remote.gameObject.SetActive(true);

            // Set Pose
            IntPtr packet = CAPI.ovrAvatarPacket_Read(data.FrameDataSize, data.FrameData);
            OvrAvatarPacket avatarPacket = new OvrAvatarPacket { ovrNativePacket = packet };
            remote.GetComponent<OvrAvatarRemoteDriver>().QueuePacket(PacketSequence++, avatarPacket);
        }
    }
}
