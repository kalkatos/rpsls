using Kalkatos.Network;
using System;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance;

        public static event Action OnFindMatchFailure;

        private RpslsGameSettings settings;

        public bool IsConnected => NetworkManager.Instance.IsConnected;

        private void Awake ()
        {
            Instance = this;
            settings = RpslsGameSettings.Instance;
            NetworkManager.OnFindMatchSuccess += HandleFindMatchSuccess;
            NetworkManager.OnFindMatchFailure += HandleFindMatchFailure;
        }

        private void OnDestroy ()
        {
            NetworkManager.OnFindMatchSuccess -= HandleFindMatchSuccess;
            NetworkManager.OnFindMatchFailure -= HandleFindMatchFailure;
        }

        private void HandleFindMatchSuccess (object obj)
        {
            SceneManager.EndScene("Lobby");
        }

        private void HandleFindMatchFailure (object obj)
        {
            OnFindMatchFailure?.Invoke();
        }

        public void FindMatch (string lobbyName = "")
        {
            if (string.IsNullOrEmpty(lobbyName))
            {
                RoomOptions options = new RoomOptions() { MaxPlayers = settings.MaxPlayers };
                NetworkManager.Instance.FindMatch(options);
            }
            else
                NetworkManager.Instance.FindMatch(lobbyName);
        }

        public void SendPlayerName (string name)
        {
            NetworkManager.Instance.SetPlayerName(name);
        }
    }
}
