using System;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;

namespace Kalkatos.Rpsls
{
    public class GameManagerClient : MonoBehaviour
    {
        public static GameManagerClient Instance { get; private set; }

        public static event Action<List<PlayerInfo>> OnPlayerListReceived;
        public static event Action<TournamentInfo> OnTournamentUpdated;

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private static RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        private void Awake ()
        {
            Instance = this;
            NetworkManager.OnEventReceived += HandleEventReceived;
        }

        private void OnDestroy ()
        {
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

        private void HandleEventReceived (string key, object[] parameters)
        {
            this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            switch (key)
            {
                case Keys.ServerPing:
                    if (paramDict.TryGetValue(Keys.PingSentId, out object pingId))
                        NetworkManager.Instance.ExecuteEvent(Keys.ResponsePing, Keys.PingSentId, byte.Parse(pingId.ToString()), Keys.PingRespondantInfo, myInfo.Id);
                    break;
                case Keys.TournamentUpdate:
                    if (paramDict.TryGetValue(Keys.TournamentInfo, out object value))
                    {
                        this.Log($"Tournament received: ({value})" + JsonConvert.SerializeObject(value));
                        TournamentInfo tournament = JsonConvert.DeserializeObject<TournamentInfo>(value.ToString());
                        //OnTournamentUpdated?.Invoke(tournament);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.TournamentInfo);
                    break;
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Game");
            NetworkManager.Instance.LeaveMatch();
        }
    }

    internal static class Keys
    {
        //Dictionary keys
        public const string TournamentInfo = "TInfo";
        public const string PingRespondantInfo = "PiRsp";
        public const string PingSentId = "PngId";
        //Events
        public const string TournamentUpdate = "TmtUp";
        public const string ServerPing = "SPing";
        public const string ResponsePing = "RPing";
    }

    public class MatchInfo
    {
        public int Id;
        public PlayerInfo Player1;
        public PlayerInfo Player2;
        public int Player1Wins;
        public int Player2Wins;
    }

    public class TournamentInfo
    {
        public MatchInfo[] Matches;
    }
}
