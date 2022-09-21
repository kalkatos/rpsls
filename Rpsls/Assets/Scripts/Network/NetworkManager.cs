using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kalkatos.Network
{
    public abstract class NetworkManager : MonoBehaviour
    {
        private static NetworkManager instance;
        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<NetworkManager>();
                return instance;
            }
        }

        public static event Action<object> OnLoginSuccess;
        public static event Action<object> OnLoginFailure;
        public static event Action<object> OnFindMatchSuccess;
        public static event Action<FindMatchError> OnFindMatchFailure;
        public static event Action<object[]> OnSendDataSuccess;
        public static event Action<object[]> OnSendDataFailure;
        public static event Action<object[]> OnRequestDataSuccess;
        public static event Action<object[]> OnRequestDataFailure;
        public static event Action<string, object[]> OnEventReceived;
        public static event Action<PlayerInfo> OnPlayerEnteredRoom;
        public static event Action<PlayerInfo> OnPlayerLeftRoom;
        public static event Action<PlayerInfo> OnPlayerDataChanged;
        public static event Action<RoomInfo> OnRoomDataChanged;
        public static event Action<PlayerInfo> OnMasterClientChanged;
        public static event Action<object[]> OnRoomOpened;
        public static event Action<object[]> OnRoomClosed;
        public static event Action<object[]> OnExecuteFunctionSuccess;
        public static event Action<object[]> OnExecuteFunctionFailure;

        public virtual bool IsConnected { get; protected set; } = false;
        public virtual bool IsInRoom { get; protected set; } = false;
        public virtual PlayerInfo MyPlayerInfo { get; protected set; } = new PlayerInfo();
        public virtual RoomInfo CurrentRoomInfo { get; protected set; } = new RoomInfo();
        public abstract DataAccess DataAccess { get; }

        private void Awake ()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        protected virtual void OnAwake () { }

        protected string CreateGuestName ()
        {
            string guestName = "Guest-";
            for (int i = 0; i < 7; i++)
                guestName += Random.Range(0, 10);
            return guestName;
        }
        protected string CreateRandomRoomName ()
        {
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            for (int i = 0; i < 6; i++)
                result += letters[Random.Range(0, letters.Length)];
            return result;
        }

        #region ==================  Requests  ===========================

        public virtual void Connect () => Debug.LogError("Connect not implemented!");
        public virtual string GetPlayerName () => CreateGuestName();
        public virtual void SetPlayerName (string name) => Debug.LogError("SetPlayerName not implemented!");
        public virtual void LogIn (object parameter = null) => Debug.LogError("LogIn not implemented!");
        public virtual void FindMatch (object parameter = null) => Debug.LogError("FindMatch not implemented!");
        public virtual void LeaveMatch (object parameter = null) => Debug.LogError("LeaveMatch not implemented!");
        public virtual void UpdateMyCustomData (params object[] parameters) => Debug.LogError("UpdateMyCustomData not implemented!");
        public virtual void UpdateRoomData (params object[] parameters) => Debug.LogError("SetRoomData not implemented!");
        public virtual void SendCustomData (params object[] parameters) => Debug.LogError("SendData not implemented!");
        public virtual void RequestCustomData (params object[] parameters) { Debug.LogError("RequestData not implemented!"); }
        public virtual void BroadcastEvent (string eventKey, params object[] parameters) { Debug.LogError("ExecuteEvent not implemented!"); }
        public virtual void OpenRoom (params object[] parameters) { Debug.LogError("OpenRoom not implemented!"); }
        public virtual void CloseRoom (params object[] parameters) { Debug.LogError("CloseRoom not implemented!"); }
        public virtual void ExecuteFunction (string functionName, object parameter) => Debug.LogError("ExecuteFunction not implemented!");
        
        #endregion

        #region ==================  Callbacks  ===========================

        public virtual void RaiseLogInSuccess (object parameter = null) { OnLoginSuccess?.Invoke(parameter); }
        public virtual void RaiseLogInFailure (object parameter = null) { OnLoginFailure?.Invoke(parameter); }
        public virtual void RaiseFindMatchSuccess (object parameter = null) { OnFindMatchSuccess?.Invoke(parameter); }
        public virtual void RaiseFindMatchFailure (FindMatchError error) { OnFindMatchFailure?.Invoke(error); }
        public virtual void RaiseSendDataSuccess (params object[] parameter) { OnSendDataSuccess?.Invoke(parameter); }
        public virtual void RaiseSendDataFailure (params object[] parameter) { OnSendDataFailure?.Invoke(parameter); }
        public virtual void RaiseRequestDataSuccess (params object[] parameter) { OnRequestDataSuccess?.Invoke(parameter); }
        public virtual void RaiseRequestDataFailure (params object[] parameter) { OnRequestDataFailure?.Invoke(parameter); }
        public virtual void RaiseEventReceived (string eventKey, params object[] parameters) { OnEventReceived?.Invoke(eventKey, parameters); }
        public virtual void RaisePlayerEnteredRoom (PlayerInfo playerInfo) { OnPlayerEnteredRoom?.Invoke(playerInfo); }
        public virtual void RaisePlayerLeftRoom (PlayerInfo playerInfo) { OnPlayerLeftRoom?.Invoke(playerInfo); }
        public virtual void RaisePlayerDataChanged (PlayerInfo playerInfo) { OnPlayerDataChanged?.Invoke(playerInfo); }
        public virtual void RaiseRoomDataChanged (RoomInfo roomInfo) { OnRoomDataChanged?.Invoke(roomInfo); }
        public virtual void RaiseMasterClientChanged (PlayerInfo newMaster) => OnMasterClientChanged?.Invoke(newMaster);
        public virtual void RaiseRoomOpened (params object[] parameters) => OnRoomOpened?.Invoke(parameters);
        public virtual void RaiseRoomClosed (params object[] parameters) => OnRoomClosed?.Invoke(parameters);
        public virtual void RaiseExecuteFunctionSuccess (params object[] parameters) => OnExecuteFunctionSuccess?.Invoke(parameters);
        public virtual void RaiseExecuteFunctionFailure (params object[] parameters) => OnExecuteFunctionFailure?.Invoke(parameters);

        #endregion
    }

    public enum FindMatchError
    {
        Unknown,
        WrongParameters,
        RoomNotFound,
        RoomIsClosed,
        RoomIsFull,
    }

    [Serializable]
    public class PlayerInfo
    {
        public string Id;
        public string Nickname;
        public bool IsMasterClient;
        public bool IsMe;
        public bool IsBot;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();

        public PlayerInfo () { }

        public PlayerInfo (PlayerInfo other)
        {
            Id = other.Id;
            Nickname = other.Nickname;
            IsMasterClient = other.IsMasterClient;
            IsMe = other.IsMe;
            CustomData = other.CustomData;
        }
    }

    [Serializable]
    public class RoomOptions
    {
        public string DesiredName;
        public int MaxPlayers;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }

    [Serializable]
    public class RoomInfo
    {
        public string Id;
        public int MaxPlayers;
        public bool IsClosed;
        public List<PlayerInfo> Players;
        public int PlayerCount;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }
}
