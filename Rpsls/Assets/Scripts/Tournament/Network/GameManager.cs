using System;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class GameManager : Client
    {
        public static GameManager Instance { get; private set; }

        public static event Action<PlayerInfo[]> OnPlayerListUpdated;
        public static event Action<RoundInfo> OnRoundReceived;
        public static event Action<string> OnStateChanged;
        public static event Action OnHandReceived;
        public static event Action<RoundInfo> OnTurnResultReceived;
        public static event Action<MatchInfo> OnMyMatchResultReceived;
        public static event Action OnTournamentEnded;

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private static RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        protected override void OnAwake ()
        {
            Instance = this;
        }

        private void Start ()
        {
            List<PlayerInfo> playerList = new List<PlayerInfo>();
            playerList.AddRange(NetworkManager.Instance.Players);
            for (int i = 0; i < playerList.Count; i++)
            {
                PlayerInfo info = playerList[i];
                players.Add(info.Id, info);
            }
            OnPlayerListUpdated?.Invoke(playerList.ToArray());
            SetId(myInfo.Id);
            SetStateAsInGame();
        }

        protected override void SetState (string state, string addInfo = "")
        {
            base.SetState(state, addInfo);
            NetworkManager.Instance.UpdateMyCustomData(Keys.ClientStateKey, currentState);
            OnStateChanged?.Invoke(state);
        }

        public override void SetRound (RoundInfo roundInfo)
        {
            base.SetRound(roundInfo);
            OnRoundReceived?.Invoke(roundInfo);
        }

        public override void SetHand ()
        {
            // TODO Actually set a hand
            base.SetHand();
            OnHandReceived?.Invoke();
        }

        public override void HandleTurnResult (RoundInfo roundInfo)
        {
            base.HandleTurnResult(roundInfo);
            OnTurnResultReceived?.Invoke(roundInfo);
            foreach (var item in roundInfo.Matches)
            {
                if (item.Player1 == Id || item.Player2 == Id)
                {
                    OnMyMatchResultReceived?.Invoke(item);
                    break;
                }
            }
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Game");
            NetworkManager.Instance.LeaveMatch();
        }

        public static void UpdatePlayers ()
        {
            if (Instance == null)
                return;
            Instance.players.Clear();
            foreach (var item in NetworkManager.Instance.Players)
                Instance.players.Add(item.Id, item);
        }

        public static PlayerInfo GetPlayer (string id)
        {
            return Instance?.players[id];
        }

        public override void HandleTournamentEnded ()
        {
            this.Log("Tournament ended.");
            OnTournamentEnded?.Invoke();
        }
    }

    public static class ClientState
    {
        public const string Undefined = "Undefined";
        public const string Disconnected = "Disconnected";
        public const string InGame = "InGame"; // In the game scene
        public const string InMatch = "InMatch"; // Finished tournament intro
        public const string InTurn = "InTurn"; // Finished card delivery intro (mulligan)
        public const string HandReceived = "HandReceived";
        public const string WaitingTurnResult = "Waiting";
        public const string BetweenRounds = "Between";
        public const string GameOver = "GameOver";
    }

    public class MatchInfo
    {
        public string Id;
        public string Player1;
        public string Player2;
        public int Player1Wins;
        public int Player2Wins;
        public bool IsOver;

        public MatchInfo () { }

        public MatchInfo (MatchInfo other)
        {
            Id = other.Id;
            Player1 = other.Player1;
            Player2 = other.Player2;
            Player1Wins = other.Player1Wins;
            Player2Wins = other.Player2Wins;
            IsOver = other.IsOver;
        }
    }

    public class RoundInfo
    {
        public string Id;
        public int Index;
        public bool IsOver;
        public MatchInfo[] Matches;

        public RoundInfo () { }

        public RoundInfo (RoundInfo other)
        {
            Id = other.Id;
            Index = other.Index;
            IsOver = other.IsOver;
            Matches = new MatchInfo[other.Matches.Length];
            for (int i = 0; i < other.Matches.Length; i++)
                Matches[i] = new MatchInfo(other.Matches[i]);
        }
    }

    public class TournamentInfo
    {
        public string Id;
        public bool IsOver;
        public string[] Players;
        public RoundInfo[] Rounds;

        public TournamentInfo () { }
        public TournamentInfo (TournamentInfo other)
        {
            Id = other.Id;
            IsOver = other.IsOver;
            Players = new string[other.Players.Length];
            for (int i = 0; i < other.Players.Length; i++)
                Players[i] = other.Players[i];
            Rounds = new RoundInfo[other.Rounds.Length];
            for (int i = 0; i < other.Rounds.Length; i++)
                Rounds[i] = new RoundInfo(other.Rounds[i]);
        }
    }

    public class MatchState
    {
        public object[] Data;
    }
}
