using Kalkatos.Network;
using System;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance;

        public static event Action<FindMatchError> OnFindMatchError;

        private RpslsGameSettings settings;
        private bool busy;

        public static bool IsConnected => NetworkManager.Instance.IsConnected;

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

        private void HandleFindMatchFailure (FindMatchError error)
        {
            busy = false;
            OnFindMatchError?.Invoke(error);
        }

        public static void FindMatch (string lobbyName = "")
        {
            if (Instance.busy)
                return;
            Instance.busy = true;
            if (string.IsNullOrEmpty(lobbyName))
            {
                RoomOptions options = new RoomOptions() { MaxPlayers = Instance.settings.MaxPlayers };
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
