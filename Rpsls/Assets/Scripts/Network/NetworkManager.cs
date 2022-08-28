using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kalkatos.Network
{
    public class NetworkManager : MonoBehaviour
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
        public static event Action<byte, object> OnEventReceived;
        public static event Action<PlayerInfo> OnPlayerEnteredLobby;
        public static event Action<PlayerInfo> OnPlayerLeftLobby;
        public static event Action<PlayerInfo, object[]> OnPlayerDataChanged;
        public static event Action<PlayerInfo> OnMasterClientChanged;

        protected Dictionary<string, object> data = new Dictionary<string, object>();

        public virtual bool IsConnected => false;
        public virtual PlayerInfo MyPlayerInfo { get; protected set; }
        public virtual LobbyInfo CurrentLobbyInfo { get; protected set; }

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
        public virtual void SendData (params object[] parameters) => Debug.LogError("SendData not implemented!");
        public virtual void RequestData (params object[] parameters) { Debug.LogError("RequestData not implemented!"); }
        public virtual void ExecuteEvent (byte eventKey, params object[] parameters) { Debug.LogError("ExecuteEvent not implemented!"); }

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
        public virtual void RaiseEventReceived (byte eventKey, object parameter = null) { OnEventReceived?.Invoke(eventKey, parameter); }
        public virtual void RaisePlayerEnteredLobby (PlayerInfo playerInfo) { OnPlayerEnteredLobby?.Invoke(playerInfo); }
        public virtual void RaisePlayerLeftLobby (PlayerInfo playerInfo) { OnPlayerLeftLobby?.Invoke(playerInfo); }
        public virtual void RaisePlayerDataChanged (PlayerInfo playerInfo, params object[] parameters) { OnPlayerDataChanged?.Invoke(playerInfo, parameters); }
        public virtual void RaiseMasterClientChanged (PlayerInfo newMaster) => OnMasterClientChanged?.Invoke(newMaster);

        #endregion
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string Id;
        public string Nickname;
        public bool IsMasterClient;
        public bool IsMe;
        public object CustomData;
    }

    [Serializable]
    public struct LobbyOptions
    {
        public int MaxPlayers;
        public object CustomData;
    }

    [Serializable]
    public struct LobbyInfo
    {
        public string Id;
        public int MaxPlayers;
        public List<PlayerInfo> Players;
        public int PlayerCount;
        public object CustomData;
    }
}
