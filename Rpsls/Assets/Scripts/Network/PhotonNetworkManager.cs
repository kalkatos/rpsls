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

        internal static PlayerInfo InfoFromPlayer (Player player)
        {
            return new PlayerInfo()
            {
                Id = player.UserId,
                Nickname = player.NickName,
                IsMasterClient = player.IsMasterClient,
                IsMe = player.IsLocal,
                CustomData = player.CustomProperties.ToDictionary()
            };
        }



        public void OnConnectedToMaster ()
        {
            PhotonNetwork.JoinLobby();
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

        // --- Photon callbacks ---

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

        #endregion
    }

    public static class PhotonConversionExtension
    {
        public static Dictionary<string, object> ToDictionary (this Hashtable hashtable)
        {
            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            foreach (var item in hashtable)
                resultDict.Add(item.Key.ToString(), item.Value);
            return resultDict;
        }
    }
}
