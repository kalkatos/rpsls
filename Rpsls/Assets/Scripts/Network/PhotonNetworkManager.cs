using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;

namespace Kalkatos.Rpsls
{
    public class PhotonNetworkManager : NetworkManager, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks
    {
        private PhotonView photonView;

        public override bool IsConnected => PhotonNetwork.IsConnected;

        protected override void OnAwake ()
        {
            photonView = gameObject.AddComponent<PhotonView>();
        }

        public virtual void OnEnable ()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public virtual void OnDisable ()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private PlayerInfo InfoFromPlayer (Player player)
        {
            return new PlayerInfo()
            {
                Id = player.UserId,
                Nickname = player.NickName,
                IsMasterClient = player.IsMasterClient,
                IsMe = player.IsLocal,
            };
        }

        private RoomOptions ConvertToRoomOptions (LobbyOptions lobbyOptions)
        {
            return new RoomOptions()
            {
                MaxPlayers = (byte)lobbyOptions.MaxPlayers
            };
        }

        #region ==================  Requests  ===========================

        public override void Connect () 
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override string GetPlayerName () => CreateGuestName();

        public override void SetPlayerName (string name)
        {
            PhotonNetwork.LocalPlayer.NickName = name;
        }

        public override void LogIn (object parameter = null) { }

        public override void FindMatch (object parameter = null)
        {
            if (parameter != null)
            {
                if (parameter is LobbyOptions)
                {
                    LobbyOptions lobbyOptions = (LobbyOptions)parameter;
                    if (string.IsNullOrEmpty(lobbyOptions.Id))
                    {
                        RoomOptions roomOptions = ConvertToRoomOptions((LobbyOptions)parameter);
                        roomOptions.PublishUserId = true;
                        PhotonNetwork.CreateRoom(CreateRandomRoomName(), roomOptions);
                    }
                    else
                        PhotonNetwork.JoinRoom(lobbyOptions.Id);
                }
                else
                {
                    Debug.LogError("Find Match expects a LobbyOptions object as parameter but it is something else.");
                    RaiseFindMatchFailure(); 
                }
            }
            else
            {
                Debug.LogError("Find Match expects a LobbyOptions object as parameter but it is null.");
                RaiseFindMatchFailure();
            }
        }

        public override void SendData (string key, params object[] parameters)
        {
            Debug.LogError("SendData not implemented!");
        }

        public override void RequestData (string key, params object[] parameters)
        {
            Debug.LogError("RequestData not implemented!"); 
        }

        public override void ExecuteEvent (string key, params object[] parameters) 
        {
            Debug.LogError("ExecuteEvent not implemented!"); 
        }

        #endregion

        #region ==================  Callbacks  ===========================
        
        public void OnConnectedToMaster ()
        {
            PhotonNetwork.JoinLobby();
        }

        public void OnJoinedLobby ()
        {
            RaiseLogInSuccess();
        }

        public void OnJoinedRoom ()
        {
            RaiseFindMatchSuccess();
        }

        public void OnPlayerEnteredRoom (Player newPlayer) 
        {
            RaisePlayerEnteredLobby(InfoFromPlayer(newPlayer));
        }

        public void OnPlayerLeftRoom (Player otherPlayer) 
        {
            RaisePlayerLeftLobby(InfoFromPlayer(otherPlayer));
        }

        public void OnMasterClientSwitched (Player newMasterClient)
        {
            RaiseEventReceived(Keys.MasterClientSwitchEvt, InfoFromPlayer(newMasterClient));
        }

        public void OnJoinRoomFailed (short returnCode, string message)
        {
            RaiseFindMatchFailure(message);
        }

        public void OnConnected () { }
        public void OnDisconnected (DisconnectCause cause) { }
        public void OnRegionListReceived (RegionHandler regionHandler) { }
        public void OnCustomAuthenticationResponse (Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed (string debugMessage) { }
        public void OnFriendListUpdate (List<FriendInfo> friendList) { }
        public void OnCreatedRoom () { }
        public void OnCreateRoomFailed (short returnCode, string message) { }
        public void OnJoinRandomFailed (short returnCode, string message) { }
        public void OnRoomPropertiesUpdate (Hashtable propertiesThatChanged) { }
        public void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps) { }
        public void OnLeftLobby () { }
        public void OnLeftRoom () { }
        public void OnRoomListUpdate (List<RoomInfo> roomList) { }
        public void OnLobbyStatisticsUpdate (List<TypedLobbyInfo> lobbyStatistics) { }
        #endregion
    }
}
