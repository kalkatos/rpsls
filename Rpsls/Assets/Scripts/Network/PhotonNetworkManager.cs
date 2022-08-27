using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;

namespace Kalkatos.Rpsls
{
    public class PhotonNetworkManager : NetworkManager, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks
    {
        [SerializeField] private PhotonView photonView;

        private RpslsGameSettings settings = RpslsGameSettings.Instance;

        public override bool IsConnected => PhotonNetwork.IsConnected;

        protected override void OnAwake ()
        {
            if (photonView == null)
                if (!TryGetComponent(out photonView))
                    photonView = gameObject.AddComponent<PhotonView>();
        }

        private void Start ()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public virtual void OnEnable ()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public virtual void OnDisable ()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        internal static PlayerInfo InfoFromPlayer (Player player, RoomStatus status = RoomStatus.Idle)
        {
            return new PlayerInfo()
            {
                Id = player.UserId,
                Nickname = player.NickName,
                IsMasterClient = player.IsMasterClient,
                IsMe = player.IsLocal,
                CustomData = status
            };
        }

        #region ==================  Requests  ===========================

        public override void SetPlayerName (string name)
        {
            PhotonNetwork.LocalPlayer.NickName = name;
        }

        public override void FindMatch (object parameter = null)
        {
            if (parameter == null)
            {
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.MaxPlayers = (byte)settings.MaxPlayers;
                roomOptions.PublishUserId = true;
                PhotonNetwork.CreateRoom(CreateRandomRoomName(), roomOptions);
            }
            PhotonNetwork.JoinRoom((string)parameter);
        }

        #endregion

        #region ==================  Callbacks  ===========================

        public override void RaiseLogInSuccess (object parameter = null)
        {
            base.RaiseLogInSuccess(parameter);
            SceneManager.EndScene("Connection");
        }

        public override void RaiseFindMatchSuccess (object parameter = null)
        {
            SessionData.RoomName = PhotonNetwork.CurrentRoom.Name;
            SessionData.IAmMasterClient = PhotonNetwork.IsMasterClient;
            base.RaiseFindMatchSuccess(parameter);
            SceneManager.EndScene("Lobby");
        }

        // --- Photon ---

        public void OnConnectedToMaster ()
        {
            PhotonNetwork.JoinLobby();
        }

        public void OnJoinedRoom ()
        {
            RoomStatus status = RoomStatus.Idle;
            if (PhotonNetwork.IsMasterClient)
                status = RoomStatus.Master;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { Keys.RoomStatus, status } });
        }

        public void OnLeftRoom ()
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { Keys.RoomStatus, RoomStatus.Idle } });
        }

        public void OnMasterClientSwitched (Player newMasterClient)
        {
            bool iBecameMaster = newMasterClient.IsLocal;
            SessionData.IAmMasterClient = iBecameMaster;
            if (iBecameMaster)
                RaiseEventReceived(Keys.IBecameMasterEvt);
        }

        public void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(Keys.RoomStatus))
                RaiseEventReceived(Keys.ChangedPlayerPropertiesEvt, InfoFromPlayer(targetPlayer));
        }

        public void OnConnected () { }
        public void OnDisconnected (DisconnectCause cause) { }
        public void OnRegionListReceived (RegionHandler regionHandler) { }
        public void OnCustomAuthenticationResponse (Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed (string debugMessage) { }
        public void OnFriendListUpdate (List<FriendInfo> friendList) { }
        public void OnCreatedRoom () { }
        public void OnCreateRoomFailed (short returnCode, string message) { }
        public void OnJoinRoomFailed (short returnCode, string message) { }
        public void OnJoinRandomFailed (short returnCode, string message) { }
        public void OnPlayerEnteredRoom (Player newPlayer) { }
        public void OnPlayerLeftRoom (Player otherPlayer) { }
        public void OnRoomPropertiesUpdate (Hashtable propertiesThatChanged) { }
        public void OnJoinedLobby () { }
        public void OnLeftLobby () { }
        public void OnRoomListUpdate (List<RoomInfo> roomList) { }
        public void OnLobbyStatisticsUpdate (List<TypedLobbyInfo> lobbyStatistics) { }
        #endregion
    }
}
