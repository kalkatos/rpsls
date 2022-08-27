using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kalkatos.Rpsls
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
        public static event Action<string, object> OnSendDataSuccess;
        public static event Action<string, object> OnSendDataFailure;
        public static event Action<string, object> OnRequestDataSuccess;
        public static event Action<string, object> OnRequestDataFailure;
        public static event Action<string, object> OnEventReceived;
        public static event Action<PlayerInfo> OnPlayerEnteredLobby;
        public static event Action<PlayerInfo> OnPlayerLeftLobby;

        protected Dictionary<string, object> data = new Dictionary<string, object>();

        public virtual bool IsConnected => false;

        private void Awake ()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
            {
                Destroy(this);
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

        public virtual string GetPlayerName () => CreateGuestName();
        public virtual void SetPlayerName (string name) => Debug.LogError("SetPlayerName not implemented!");
        public virtual void LogIn (object parameter = null) => Debug.LogError("LogIn not implemented!");
        public virtual void FindMatch (object parameter = null) => Debug.LogError("FindMatch not implemented!");
        public virtual void SendData (string key, params object[] parameters) => Debug.LogError("SendData not implemented!");
        public virtual void RequestData (string key, params object[] parameters) { Debug.LogError("RequestData not implemented!"); }
        public virtual void ExecuteEvent (string key, params object[] parameters) { Debug.LogError("ExecuteEvent not implemented!"); }

        #endregion

        #region ==================  Callbacks  ===========================

        public virtual void RaiseLogInSuccess (object parameter = null) { OnLoginSuccess?.Invoke(parameter); }
        public virtual void RaiseLogInFailure (object parameter = null) { OnLoginFailure?.Invoke(parameter); }
        public virtual void RaiseFindMatchSuccess (object parameter = null) { OnFindMatchSuccess?.Invoke(parameter); }
        public virtual void RaiseFindMatchFailure (object parameter = null) { OnFindMatchFailure?.Invoke(parameter); }
        public virtual void RaiseSendDataSuccess (string key, object parameter = null) { OnSendDataSuccess?.Invoke(key, parameter); }
        public virtual void RaiseSendDataFailure (string key, object parameter = null) { OnSendDataFailure?.Invoke(key, parameter); }
        public virtual void RaiseRequestDataSuccess (string key, object parameter = null) { OnRequestDataSuccess?.Invoke(key, parameter); }
        public virtual void RaiseRequestDataFailure (string key, object parameter = null) { OnRequestDataFailure?.Invoke(key, parameter); }
        public virtual void RaiseEventReceived (string key, object parameter = null) { OnEventReceived?.Invoke(key, parameter); }
        public virtual void RaisePlayerEnteredLobby (PlayerInfo playerInfo) { OnPlayerEnteredLobby?.Invoke(playerInfo); }
        public virtual void RaisePlayerLeftLobby (PlayerInfo playerInfo) { OnPlayerLeftLobby?.Invoke(playerInfo); }

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
}
