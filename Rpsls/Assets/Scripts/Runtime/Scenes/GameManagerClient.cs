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

        private string myId;
        private ClientState currentState;
        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private TournamentInfo tournament;

        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private static RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        private void Awake ()
        {
            Instance = this;
            myId = myInfo.Id;
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
            currentState = ClientState.GameReady;
            NetworkManager.Instance.SendData($"{Keys.ClientCheckKey}-{myId}", (int)currentState);
        }

        private void HandleEventReceived (string key, object[] parameters)
        {
            this.Log("Event received in client: " + key);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            switch (key)
            {
                case Keys.TournamentUpdateEvt:
                    if (paramDict.TryGetValue(Keys.TournamentInfoKey, out object value))
                    {
                        this.Log($"Tournament received:     {value}");
                        tournament = JsonConvert.DeserializeObject<TournamentInfo>(value.ToString());
                        OnTournamentUpdated?.Invoke(tournament);
                    }
                    else
                        this.LogWarning("Didn't receive the key " + Keys.TournamentInfoKey);
                    break;
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Game");
            NetworkManager.Instance.LeaveMatch();
        }
    }

    public enum ClientState
    {
        Undefined,
        GameReady,
        MatchPresentation,

    }

    internal static class Keys
    {
        //Dictionary keys
        public const string TournamentInfoKey = "TInfo";
        public const string ClientCheckKey = "CtRdy";
        //Events
        public const string TournamentUpdateEvt = "TmtUp";
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
