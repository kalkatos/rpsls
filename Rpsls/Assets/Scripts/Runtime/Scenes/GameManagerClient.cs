using System;
using System.Collections.Generic;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class GameManagerClient : Client
    {
        public static GameManagerClient Instance { get; private set; }

        public static event Action<List<PlayerInfo>> OnPlayerListReceived;
        public static event Action<TournamentInfo> OnTournamentUpdated;

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        private static PlayerInfo myInfo => NetworkManager.Instance.MyPlayerInfo;
        private static RoomInfo roomInfo => NetworkManager.Instance.CurrentRoomInfo;

        protected override void OnAwake ()
        {
            Instance = this;
            SetInfo(myInfo);
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
            SetReadyInGame();
        }

        public override void SetTournament (TournamentInfo tournamentInfo)
        {
            base.SetTournament(tournamentInfo);
            OnTournamentUpdated?.Invoke(tournamentInfo);
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

    }

    internal static class Keys
    {
        //Dictionary keys
        public const string TournamentInfoKey = "TInfo";
        public const string ClientIdKey = "PlrId";
        //Events
        public const string TournamentUpdateEvt = "TmtUp";
        public const string TurnUpdateEvt = "TuUpt";
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
