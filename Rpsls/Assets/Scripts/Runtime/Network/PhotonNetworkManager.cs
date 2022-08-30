﻿using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Kalkatos.Network
{
    public class PhotonNetworkManager : NetworkManager, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IOnEventCallback
    {
        private List<string> functionNamesToByte = new List<string>() { "xreservex" };

        public override bool IsConnected => PhotonNetwork.IsConnected;

        public virtual void OnEnable ()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public virtual void OnDisable ()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override PlayerInfo MyPlayerInfo
        {
            get => InfoFromPlayer(PhotonNetwork.LocalPlayer);
            protected set => base.MyPlayerInfo = value; 
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
                }
                return info;
            }
            protected set => base.CurrentRoomInfo = value;
        }

        private PlayerInfo InfoFromPlayer (Player player)
        {
            return new PlayerInfo()
            {
                Id = player.UserId,
                Nickname = player.NickName,
                IsMasterClient = player.IsMasterClient,
                CustomData = player.CustomProperties.ToDictionary()
            };
        }

        private Photon.Realtime.RoomOptions ConvertToRoomOptions (RoomOptions lobbyOptions)
        {
            return new Photon.Realtime.RoomOptions()
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
                if (parameter is RoomOptions)
                {
                    Photon.Realtime.RoomOptions roomOptions = ConvertToRoomOptions((RoomOptions)parameter);
                    roomOptions.PublishUserId = true;
                    PhotonNetwork.CreateRoom(CreateRandomRoomName(), roomOptions);
                }
                else if (parameter is string)
                    PhotonNetwork.JoinRoom((string)parameter);
                else
                {
                    Debug.LogError("Find Match expects a LobbyOptions or string object as parameter but it is something else.");
                    RaiseFindMatchFailure(); 
                }
            }
            else
            {
                Debug.LogError("Find Match expects a LobbyOptions or string object as parameter but it is null.");
                RaiseFindMatchFailure();
            }
        }

        public override void LeaveMatch (object parameter = null)
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void SendData (params object[] parameters)
        {
            PhotonNetwork.LocalPlayer.CustomProperties = parameters.ToHashtable();
        }

        public override void RequestData (params object[] parameters)
        {
            
        }

        public override void ExecuteEvent (string eventKey, params object[] parameters) 
        {
            byte byteKey;
            if (functionNamesToByte.Contains(eventKey))
                byteKey = (byte)functionNamesToByte.IndexOf(eventKey);
            else
            {
                byteKey = (byte)functionNamesToByte.Count;
                functionNamesToByte.Add(eventKey);
            }
            PhotonNetwork.RaiseEvent(byteKey, parameters, RaiseEventOptions.Default, SendOptions.SendReliable);
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
            RaiseFindMatchFailure(message);
        }

        public void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps) 
        {
            RaisePlayerDataChanged(InfoFromPlayer(targetPlayer), changedProps?.ToDictionary());
        }

        public void OnEvent (EventData photonEvent)
        {
            RaiseEventReceived(functionNamesToByte[photonEvent.Code], (object[])photonEvent.CustomData);
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
}