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

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private RpslsGameSettings settings;
        private RoomInfo roomInfo;

        private const string setRoomStatusKey = "Srsts";

        public static string RoomName { get; private set; }
        public static bool IAmTheMaster { get; private set; }
        public static string MyId { get; private set; }
        private PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;

        private void Awake ()
        {
            NetworkManager.OnPlayerEnteredRoom += HandlePlayerEntered;
            NetworkManager.OnPlayerLeftRoom += HandlePlayerLeft;
            NetworkManager.OnMasterClientChanged += HandleMasterClientChanged;
            NetworkManager.OnEventReceived += HandleEventReceived;
            Instance = this;
            settings = RpslsGameSettings.Instance;
            PlayerInfo myInfo = this.myInfo;
            IAmTheMaster = myInfo.IsMasterClient;
            MyId = myInfo.Id;
            roomInfo = GetUpdatedLobbyInfo();
            RoomName = roomInfo.Id;
            SetStatus(IAmTheMaster ? RoomStatus.Master : RoomStatus.Idle);
        }

        private void OnDestroy ()
        {
            NetworkManager.OnPlayerEnteredRoom -= HandlePlayerEntered;
            NetworkManager.OnPlayerLeftRoom -= HandlePlayerLeft;
            NetworkManager.OnMasterClientChanged -= HandleMasterClientChanged;
            NetworkManager.OnEventReceived -= HandleEventReceived;
        }

        private void Start ()
        {
            List<PlayerInfo> playerList = new List<PlayerInfo>();
            playerList.AddRange(roomInfo.Players);
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerInfo info = playerList[i];
                players.Add(info.Id, info);
            }
            OnPlayerListReceived?.Invoke(playerList);
        }

        private void HandlePlayerEntered (PlayerInfo newPlayer)
        {
            if (!players.ContainsKey(newPlayer.Id))
            {
                players.Add(newPlayer.Id, newPlayer);
                OnPlayerEntered?.Invoke(newPlayer);
            }
        }

        private void HandlePlayerLeft (PlayerInfo otherPlayer)
        {
            if (players.ContainsKey(otherPlayer.Id))
            {
                players.Remove(otherPlayer.Id);
                OnPlayerLeft?.Invoke(otherPlayer);
            }
        }

        private void HandleMasterClientChanged (PlayerInfo newMaster)
        {
            if (newMaster.Id == MyId)
            {
                IAmTheMaster = true;
                OnBecameMaster?.Invoke();
                SetStatus(RoomStatus.Master);
            }
        }

        private void HandleEventReceived (string eventKey, object[] parameters)
        {
            if (eventKey == setRoomStatusKey)
            {
                string playerId = (string)parameters[0];
                RoomStatus status = (RoomStatus)int.Parse(parameters[1].ToString());
                if (players.ContainsKey(playerId))
                {
                    PlayerInfo info = players[playerId];
                    info.CustomData = status;
                    players[playerId] = info;
                    OnPlayerStatusChanged?.Invoke(playerId, status);
                }
            }
        }

        private RoomInfo GetUpdatedLobbyInfo ()
        {
            return NetworkManager.Instance.CurrentRoomInfo;
        }

        public static void SetStatus (RoomStatus status)
        {
            NetworkManager.Instance.ExecuteEvent(setRoomStatusKey, MyId, status);
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
