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

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private static RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        protected override void OnAwake ()
        {
            Instance = this;
            SetInfo(myInfo);
        }

        private void OnDestroy ()
        {
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
            OnPlayerListUpdated?.Invoke(playerList.ToArray());
            SetReadyInGame();
        }

        public override void SetRound (RoundInfo roundInfo)
        {
            base.SetRound(roundInfo);
            OnRoundReceived?.Invoke(roundInfo);
        }

        public static void ExitRoom ()
        {
            SceneManager.EndScene("Game");
            NetworkManager.Instance.LeaveMatch();
        }
    }

    public enum ClientState
    {
        Undefined = 0,
        GameReady = -100,
        MatchReady = -50,
        TurnReady = -10
    }

    public class MatchInfo
    {
        public int Id;
        public PlayerInfo Player1;
        public PlayerInfo Player2;
        public int Player1Wins;
        public int Player2Wins;
    }

    public class RoundInfo
    {
        public string Id;
        public MatchInfo[] Matches;
    }

    public class TournamentInfo
    {
        public string Id;
        public string[] Players;
        public RoundInfo[] Rounds;
    }
}
