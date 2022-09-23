using System;
using UnityEngine;
using UnityEngine.Assertions;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Kalkatos.Network
{
    public class PhotonNetworkManager : NetworkManager, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IOnEventCallback
    {
        [SerializeField] private int numberOfBots;

        private const string lastByteUsedForEvents = "LstBt";
        private const string executeEventKey = "EvKey";
        private const byte customEvent = 1;

        private PhotonDataAccess dataAccess = new PhotonDataAccess();
        private List<PlayerInfo> botList = new List<PlayerInfo>();

        public override bool IsConnected => PhotonNetwork.IsConnected;
        public override bool IsInRoom => PhotonNetwork.CurrentRoom != null;
        public override DataAccess DataAccess => dataAccess;

        public virtual void OnEnable ()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public virtual void OnDisable ()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnApplicationQuit ()
        {
            PhotonNetwork.Disconnect();
        }

        protected override void OnAwake ()
        {
            for (int i = 0; i < numberOfBots; i++)
            {
                botList.Add(new PlayerInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    Nickname = CreateGuestName(),
                    IsBot = true
                });
            }
        }

        public override PlayerInfo MyPlayerInfo
        {
            get => InfoFromPlayer(PhotonNetwork.LocalPlayer);
        }

        public override RoomInfo CurrentRoomInfo
        {
            get
            {
                RoomInfo info = new RoomInfo();
                if (PhotonNetwork.InRoom)
                {
                    Room room = PhotonNetwork.CurrentRoom;
                    info.Id = room.Name;
                    info.MaxPlayers = room.MaxPlayers;
                    info.PlayerCount = room.PlayerCount;
                    info.Players = new List<PlayerInfo>();
                    foreach (var item in room.Players)
                        info.Players.Add(InfoFromPlayer(item.Value));
                    info.Players.AddRange(botList);
                }
                return info;
            }
        }

        public static PlayerInfo InfoFromPlayer (Player player)
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

        private Photon.Realtime.RoomOptions ConvertToRoomOptions (RoomOptions roomOptions)
        {
            return new Photon.Realtime.RoomOptions()
            {
                MaxPlayers = (byte)roomOptions.MaxPlayers
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
                if (parameter is RoomOptions)
                {
                    Photon.Realtime.RoomOptions roomOptions = ConvertToRoomOptions((RoomOptions)parameter);
                    roomOptions.PublishUserId = true;
                    roomOptions.CustomRoomProperties = new Hashtable() { { lastByteUsedForEvents, (byte)0 } };
                    PhotonNetwork.CreateRoom(CreateRandomRoomName(), roomOptions);
                }
                else if (parameter is string)
                    PhotonNetwork.JoinRoom((string)parameter);
                else
                {
                    Debug.LogError("Find Match expects a LobbyOptions or string object as parameter but it is something else.");
                    RaiseFindMatchFailure(FindMatchError.WrongParameters); 
                }
            }
            else
            {
                Debug.LogError("Find Match expects a LobbyOptions or string object as parameter but it is null.");
                RaiseFindMatchFailure(FindMatchError.WrongParameters);
            }
        }

        public override void LeaveMatch (object parameter = null)
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void UpdateMyCustomData (params object[] parameters)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(parameters.ToHashtable());
        }

        public override void UpdateRoomData (params object[] parameters)
        {
            if (IsInRoom)
                PhotonNetwork.CurrentRoom.SetCustomProperties(parameters.ToHashtable());
        }

        public override void SendCustomData (params object[] parameters)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(parameters.ToHashtable());
        }

        public override void RequestCustomData (params object[] parameters)
        {
            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            var data = PhotonNetwork.CurrentRoom.CustomProperties;
            int missedParams = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (data.TryGetValue(parameters[i].ToString(), out object value))
                    resultDict.Add(parameters[i].ToString(), value);
                else
                {
                    resultDict.Add(parameters[i].ToString(), null);
                    missedParams++;
                }
            }
            if (missedParams == parameters.Length)
                RaiseRequestDataFailure(parameters);
            else
                RaiseRequestDataSuccess(resultDict);
        }

        public override void BroadcastEvent (string eventKey, params object[] parameters) 
        {
            Assert.IsTrue(PhotonNetwork.InRoom);

            parameters = parameters.CloneWithChange(executeEventKey, eventKey);
            PhotonNetwork.RaiseEvent(customEvent, parameters, RaiseEventOptions.Default, SendOptions.SendReliable);
            RaiseEventReceived(eventKey, parameters);
        }

        public override void OpenRoom (params object[] parameters) 
        {
            Assert.IsTrue(PhotonNetwork.InRoom);

            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

        public override void CloseRoom (params object[] parameters) 
        {
            Assert.IsTrue(PhotonNetwork.InRoom);

            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        // TODO Implement Execute Function

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
            Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
            RaiseFindMatchSuccess();
        }

        void IInRoomCallbacks.OnPlayerEnteredRoom (Player newPlayer) 
        {
            RaisePlayerEnteredRoom(InfoFromPlayer(newPlayer));
        }

        void IInRoomCallbacks.OnPlayerLeftRoom (Player otherPlayer) 
        {
            RaisePlayerLeftRoom(InfoFromPlayer(otherPlayer));
        }

        public void OnMasterClientSwitched (Player newMasterClient)
        {
            RaiseMasterClientChanged(InfoFromPlayer(newMasterClient));
        }

        public void OnJoinRoomFailed (short returnCode, string message)
        {
            this.Log($"Join Room Error Received: {message} - Return code: {returnCode}");
            switch (returnCode)
            {
                case 32765:
                    RaiseFindMatchFailure(FindMatchError.RoomIsFull);
                    break;
                case 32764:
                    RaiseFindMatchFailure(FindMatchError.RoomIsClosed);
                    break;
                case 32758:
                    RaiseFindMatchFailure(FindMatchError.RoomNotFound);
                    break;
                default:
                    RaiseFindMatchFailure(FindMatchError.Unknown);
                    break;
            }
        }

        public void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps) 
        {
            RaisePlayerDataChanged(InfoFromPlayer(targetPlayer));
        }

        public void OnEvent (EventData photonEvent)
        {
            if (PhotonNetwork.InRoom)
            {
                if (photonEvent.CustomData != null)
                {
                    Dictionary<string, object> parameterDict = ((object[])photonEvent.CustomData).ToDictionary();
                    if (parameterDict.ContainsKey(executeEventKey))
                    {
                        string evtName = (string)parameterDict[executeEventKey];
                        parameterDict.Remove(executeEventKey);
                        RaiseEventReceived(evtName, parameterDict.ToObjArray());
                    }
                }
            }
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
        public void OnLeftLobby () { }
        public void OnLeftRoom () { }
        public void OnRoomListUpdate (List<Photon.Realtime.RoomInfo> roomList) { }
        public void OnLobbyStatisticsUpdate (List<TypedLobbyInfo> lobbyStatistics) { }

        #endregion
    }

    public static class PhotonCustomExtentions
    {
        public static Dictionary<string, object> ToDictionary (this Hashtable hashtable)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var item in hashtable)
                result.Add(item.Key.ToString(), item.Value);
            return result;
        }

        public static Hashtable ToHashtable (this Dictionary<string, object> dictionary)
        {
            Hashtable result = new Hashtable();
            foreach (var item in dictionary)
                result.Add(item.Key, item.Value);
            return result;
        }

        public static Hashtable ToHashtable (this object[] objArray)
        {
            Hashtable result = new Hashtable();
            for (int i = 0; i < objArray.Length; i++)
            {
                if (i % 2 == 0)
                    result.Add(objArray[i].ToString(), null);
                else
                    result[objArray[i - 1].ToString()] = objArray[i];
            }
            return result;
        }

        public static Hashtable Append (this Hashtable hashtable, object[] objArray)
        {
            for (int i = 0; i < objArray.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (!hashtable.ContainsKey(objArray[i]))
                        hashtable.Add(objArray[i].ToString(), null);
                }
                else
                    hashtable[objArray[i - 1]] = objArray[i];
            }
            return hashtable;
        }

        public static Hashtable HashtableClone (this Hashtable hashtable)
        {
            Hashtable result = new Hashtable();
            foreach (var item in hashtable)
                result.Add(item.Key, item.Value);
            return result;
        }
    }

    internal class PhotonDataAccess : DataAccess
    {
        #pragma warning disable
        public override async Task<string> RequestData (string key, string defaultValue, string container = "")
        {
            if (container == Keys.ConnectedPlayersKey) // TODO Do the same for rooms
            {
                foreach (var item in PhotonNetwork.PlayerList)
                    if (item.UserId == key)
                        if (item.CustomProperties.TryGetValue(key, out object value))
                            return value.ToString();
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value))
                return value.ToString();
            return "";
        }

        public override async Task SendData (string key, string value, string container = "")
        {
            if (container == Keys.ConnectedPlayersKey && PhotonNetwork.LocalPlayer.IsMasterClient) // TODO Do the same for rooms
            {
                foreach (var item in PhotonNetwork.PlayerList)
                    if (item.UserId == key)
                    {
                        item.SetCustomProperties(new Hashtable() { { key, value } });
                        break;
                    }
            }
            else
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { key, value } });
        }
        #pragma warning restore
    }
}
