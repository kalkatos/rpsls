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
                    instance = new GameObject("NetworkManager").AddComponent<NetworkManager>();
                return instance;
            }
        }

        public static event Action<object> OnLoginSuccess;
        public static event Action<object> OnLoginFailure;
        public static event Action<object> OnFindMatchSuccess;
        public static event Action<object> OnFindMatchFailure;
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

        public virtual bool IsConnected { get; protected set; } = false;
        public virtual bool IsInRoom { get; protected set; } = false;
        public virtual PlayerInfo MyPlayerInfo { get; protected set; }
        public virtual RoomInfo CurrentRoomInfo { get; protected set; }

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

        public virtual void Connect () { }
        public virtual string GetPlayerName () => CreateGuestName();
        public virtual void SetPlayerName (string name) => Debug.LogError("SetPlayerName not implemented!");
        public virtual void LogIn (object parameter = null) => Debug.LogError("LogIn not implemented!");
        public virtual void FindMatch (object parameter = null) => Debug.LogError("FindMatch not implemented!");
        public virtual void LeaveMatch (object parameter = null) => Debug.LogError("LeaveMatch not implemented!");
        public virtual void SetMyCustomData (Dictionary<string, object> data) => Debug.LogError("SetMyCustomData not implemented!");
        public virtual void SetRoomData (Dictionary<string, object> data) => Debug.LogError("SetRoomData not implemented!");
        public virtual void SendData (params object[] parameters) => Debug.LogError("SendData not implemented!");
        public virtual void RequestData (params object[] parameters) { Debug.LogError("RequestData not implemented!"); }
        public virtual void ExecuteEvent (string eventKey, params object[] parameters) { Debug.LogError("ExecuteEvent not implemented!"); }

        #endregion

        #region ==================  Callbacks  ===========================

        public virtual void RaiseLogInSuccess (object parameter = null) { OnLoginSuccess?.Invoke(parameter); }
        public virtual void RaiseLogInFailure (object parameter = null) { OnLoginFailure?.Invoke(parameter); }
        public virtual void RaiseFindMatchSuccess (object parameter = null) { OnFindMatchSuccess?.Invoke(parameter); }
        public virtual void RaiseFindMatchFailure (object parameter = null) { OnFindMatchFailure?.Invoke(parameter); }
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

        #endregion
    }

    [Serializable]
    public class PlayerInfo
    {
        public string Id;
        public string Nickname;
        public bool IsMasterClient;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();

        public PlayerInfo () { }

        public PlayerInfo (PlayerInfo other)
        {
            Id = other.Id;
            Nickname = other.Nickname;
            IsMasterClient = other.IsMasterClient;
            CustomData = other.CustomData;
        }
    }

    [Serializable]
    public class RoomOptions
    {
        public int MaxPlayers;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }

    [Serializable]
    public class RoomInfo
    {
        public string Id;
        public int MaxPlayers;
        public List<PlayerInfo> Players;
        public int PlayerCount;
        public Dictionary<string, object> CustomData = new Dictionary<string, object>();
    }
}
