using System;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Rpsls
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        public static event Action<List<PlayerInfo>> OnPlayerListReceived;
        public static event Action<PlayerInfo> OnPlayerEntered;
        public static event Action<PlayerInfo> OnPlayerLeft;
        public static event Action<string, RoomStatus> OnPlayerStatusChanged;
        public static event Action OnBecameMaster;
        public static event Action OnGameAboutToStart;

        private List<PlayerInfo> players = new List<PlayerInfo>();
        private RpslsGameSettings settings;
        private LobbyInfo lobbyInfo;

        public static string RoomName { get; private set; }
        public static bool IAmTheMaster { get; private set; }
        private PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;

        private void Awake ()
        {
            NetworkManager.OnPlayerEnteredLobby += HandlePlayerEntered;
            NetworkManager.OnPlayerLeftLobby += HandlePlayerLeft;
            NetworkManager.OnPlayerDataChanged += HandlePlayerDataChanged;
            NetworkManager.OnMasterClientChanged += HandleMasterClientChanged;
            Instance = this;
            settings = RpslsGameSettings.Instance;
            IAmTheMaster = myInfo.IsMasterClient;
            lobbyInfo = GetUpdatedLobbyInfo();
            RoomName = lobbyInfo.Id;
            SetStatus(IAmTheMaster ? RoomStatus.Master : RoomStatus.Idle);
        }

        private void OnDestroy ()
        {
            NetworkManager.OnPlayerEnteredLobby -= HandlePlayerEntered;
            NetworkManager.OnPlayerLeftLobby -= HandlePlayerLeft;
            NetworkManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
            NetworkManager.OnMasterClientChanged -= HandleMasterClientChanged;
        }

        private void Start ()
        {
            List<PlayerInfo> playerList = lobbyInfo.Players;
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerInfo info = playerList[i];
                players.Add(info);
            }
            OnPlayerListReceived?.Invoke(players);
        }

        private void HandlePlayerEntered (PlayerInfo newPlayer)
        {
            players.Add(newPlayer);
            OnPlayerEntered?.Invoke(newPlayer);
        }

        private void HandlePlayerLeft (PlayerInfo otherPlayer)
        {
            players.Remove(otherPlayer);
            OnPlayerLeft?.Invoke(otherPlayer);
        }

        private void HandlePlayerDataChanged (PlayerInfo playerInfo, object data)
        {
            OnPlayerStatusChanged?.Invoke(playerInfo.Id, (RoomStatus)((Dictionary<string, object>)playerInfo.CustomData)["Status"]);
        }

        private void HandleMasterClientChanged (PlayerInfo newMaster)
        {
            if (newMaster.IsMe)
            {
                IAmTheMaster = true;
                OnBecameMaster?.Invoke();
                SetStatus(RoomStatus.Master);
            }
        }

        private LobbyInfo GetUpdatedLobbyInfo ()
        {
            return NetworkManager.Instance.CurrentLobbyInfo;
        }

        public static void SetStatus (RoomStatus status)
        {
            NetworkManager.Instance.SendData("Status", status);
        }

        public static void StartGame ()
        {
            Debug.Log("Calling start game");
            if (IAmTheMaster)
            {
                OnGameAboutToStart?.Invoke();
                Instance.Wait(Instance.settings.DelayBeforeStarting, () => Debug.Log("Start!"));
                //TODO Broadcast a start game call
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Room", "ToLobby");
            NetworkManager.Instance.LeaveMatch();
        }
    }
}
