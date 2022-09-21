using System;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        public static event Action<List<PlayerInfo>> OnPlayerListReceived;
        public static event Action<PlayerInfo> OnPlayerEntered;
        public static event Action<PlayerInfo> OnPlayerLeft;
        public static event Action<PlayerInfo> OnPlayerInfoChanged;
        public static event Action OnBecameMaster;
        public static event Action OnGameAboutToStart;
        public static event Action<NotReadyErrorCode> OnGameNotReady;

        public const string RoomStatusKey = "RmSts";
        private const string startGameKey = "Start";
        private const string aboutToStartKey = "AbSrt";

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private TournamentGameSettings settings;

        public static string RoomName { get; private set; }
        public static bool IAmTheMaster => myInfo.IsMasterClient;
        public static string MyId => myInfo.Id;
        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        private void Awake ()
        {
            NetworkManager.OnPlayerEnteredRoom += HandlePlayerEntered;
            NetworkManager.OnPlayerLeftRoom += HandlePlayerLeft;
            NetworkManager.OnMasterClientChanged += HandleMasterClientChanged;
            NetworkManager.OnPlayerDataChanged += HandlePlayerDataChanged;
            NetworkManager.OnEventReceived += HandleEventReceived;
            Instance = this;
            settings = TournamentGameSettings.Instance;
            RoomName = roomInfo.Id;
        }

        private void OnDestroy ()
        {
            NetworkManager.OnPlayerEnteredRoom -= HandlePlayerEntered;
            NetworkManager.OnPlayerLeftRoom -= HandlePlayerLeft;
            NetworkManager.OnMasterClientChanged -= HandleMasterClientChanged;
            NetworkManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
            NetworkManager.OnEventReceived -= HandleEventReceived;
        }

        private void Start ()
        {
            List<PlayerInfo> playerList = new List<PlayerInfo>();
            playerList.AddRange(roomInfo.Players);
            playerList.Sort(SortPlayers);
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerInfo info = playerList[i];
                players.Add(info.Id, info);
            }
            OnPlayerListReceived?.Invoke(playerList);
            SetStatus(IAmTheMaster ? RoomStatus.Master : RoomStatus.Idle);

            int SortPlayers (PlayerInfo a, PlayerInfo b)
            {
                if (a.IsMasterClient && !b.IsMasterClient)
                    return -1;
                if (b.IsMasterClient && !a.IsMasterClient)
                    return 1;
                return 0;
            }
        }

        private void HandlePlayerEntered (PlayerInfo newPlayer)
        {
            if (!players.ContainsKey(newPlayer.Id))
            {
                players.Add(newPlayer.Id, newPlayer);
                OnPlayerEntered?.Invoke(newPlayer);
            }
        }

        private void HandlePlayerLeft (PlayerInfo player)
        {
            if (players.ContainsKey(player.Id))
            {
                players.Remove(player.Id);
                OnPlayerLeft?.Invoke(player);
            }
        }

        private void HandleMasterClientChanged (PlayerInfo newMaster)
        {
            if (newMaster.Id == MyId)
            {
                OnBecameMaster?.Invoke();
                SetStatus(RoomStatus.Master);
            }
        }

        private void HandlePlayerDataChanged (PlayerInfo player)
        {
            if (players.ContainsKey(player.Id))
            {
                OnPlayerInfoChanged?.Invoke(player);
                players[player.Id] = player;
            }
            else
                this.LogError("Received data changed from a player, but they are not in my known list of players.");
        }

        private void HandleEventReceived (string eventKey, object[] parameters)
        {
            if (eventKey == aboutToStartKey)
                OnGameAboutToStart?.Invoke();
            if (eventKey == startGameKey)
                SceneManager.EndScene("Room");
        }

        public static void SetStatus (RoomStatus status)
        {
            NetworkManager.Instance.UpdateMyCustomData(RoomStatusKey, (int)status);
        }

        public static void StartGame ()
        {
            if (!IAmTheMaster)
                return;

            if (Instance.players.Count <= 1)
            {
                OnGameNotReady?.Invoke(NotReadyErrorCode.NotEnoughPlayers);
            }
            else
            {
                bool isEveryoneReady = true;
                foreach (var item in Instance.players)
                {
                    RoomStatus playerStatus = (RoomStatus)int.Parse(item.Value.CustomData[RoomStatusKey].ToString());
                    isEveryoneReady &= playerStatus == RoomStatus.Ready || playerStatus == RoomStatus.Master;
                }
                if (isEveryoneReady)
                {
                    Instance.Log("Calling start game");
                    NetworkManager.Instance.BroadcastEvent(aboutToStartKey);
                    NetworkManager.Instance.CloseRoom();
                    //TODO NetworkManager.Instance.ExecuteFunction(Keys.StartTournamentFct, Instance.roomInfo.Id);
                    Instance.Wait(Instance.settings.DelayBeforeStarting, () =>
                    {
                        Instance.Log("Start!");
                        NetworkManager.Instance.BroadcastEvent(startGameKey);
                    });

                }
                else
                    OnGameNotReady?.Invoke(NotReadyErrorCode.NotEveryoneReady);
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Room", "ToLobby");
            NetworkManager.Instance.LeaveMatch();
        }
    }

    public enum NotReadyErrorCode { NotEnoughPlayers, NotEveryoneReady }

    public enum RoomStatus
    {
        Idle = 0,
        Ready = 1,
        Master = 2
    }
}
