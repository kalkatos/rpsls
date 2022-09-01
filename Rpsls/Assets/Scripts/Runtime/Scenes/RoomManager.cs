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
        public static event Action<NotReadyErrorCode> OnGameNotReady;

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private RpslsGameSettings settings;
        private RoomInfo roomInfo;

        private const string setRoomStatusKey = "SRSts";
        private const string startGameKey = "Start";

        public static string RoomName { get; private set; }
        public static bool IAmTheMaster { get; private set; }
        public static string MyId { get; private set; }
        private PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;

        private void Awake ()
        {
            NetworkManager.OnPlayerEnteredRoom += HandlePlayerEntered;
            NetworkManager.OnPlayerLeftRoom += HandlePlayerLeft;
            NetworkManager.OnMasterClientChanged += HandleMasterClientChanged;
            NetworkManager.OnPlayerDataChanged += HandlePlayerDataChanged;
            NetworkManager.OnEventReceived += HandleEventReceived;
            Instance = this;
            settings = RpslsGameSettings.Instance;
            PlayerInfo myInfo = this.myInfo;
            IAmTheMaster = myInfo.IsMasterClient;
            MyId = myInfo.Id;
            roomInfo = GetUpdatedRoomInfo();
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
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerInfo info = playerList[i];
                players.Add(info.Id, info);
            }
            OnPlayerListReceived?.Invoke(playerList);
            SetStatus(IAmTheMaster ? RoomStatus.Master : RoomStatus.Idle);
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
                IAmTheMaster = true;
                OnBecameMaster?.Invoke();
                SetStatus(RoomStatus.Master);
            }
        }

        private void HandlePlayerDataChanged (PlayerInfo player, object[] parameters)
        {
            if (players.ContainsKey(player.Id))
            {
                PlayerInfo infoHere = players[player.Id];
                if (player.CustomData != infoHere.CustomData)
                    OnPlayerStatusChanged?.Invoke(player.Id, (RoomStatus)int.Parse(player.CustomData.ToString()));
                players[player.Id] = player;
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
            else if (eventKey == startGameKey)
            {
                SceneManager.EndScene("Room");
            }
        }

        private RoomInfo GetUpdatedRoomInfo ()
        {
            return NetworkManager.Instance.CurrentRoomInfo;
        }

        public static void SetStatus (RoomStatus status)
        {
            NetworkManager.Instance.SetMyCustomData((int)status);
        }

        public static void StartGame ()
        {

            if (IAmTheMaster)
            {
                if (Instance.players.Count <= 1)
                {
                    OnGameNotReady?.Invoke(NotReadyErrorCode.NotEnoughPlayers);
                }
                else
                {
                    bool isEveryoneReady = true;
                    foreach (var item in Instance.players)
                    {
                        RoomStatus playerStatus = (RoomStatus)int.Parse(item.Value.CustomData.ToString());
                        isEveryoneReady &= playerStatus == RoomStatus.Ready || playerStatus == RoomStatus.Master;
                    }
                    if (isEveryoneReady)
                    {
                        Debug.Log("Calling start game");
                        OnGameAboutToStart?.Invoke();
                        Instance.Wait(Instance.settings.DelayBeforeStarting, () =>
                        {
                            Debug.Log("Start!");
                            NetworkManager.Instance.ExecuteEvent(startGameKey);
                        });
                        
                    }
                    else
                        OnGameNotReady?.Invoke(NotReadyErrorCode.NotEveryoneReady);
                }
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Room", "ToLobby");
            NetworkManager.Instance.LeaveMatch();
        }
    }

    public enum NotReadyErrorCode { NotEnoughPlayers, NotEveryoneReady }
}
