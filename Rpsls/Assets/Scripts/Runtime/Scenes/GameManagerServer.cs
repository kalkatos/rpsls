using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;

namespace Kalkatos.Rpsls
{
    public class GameManagerServer : MonoBehaviour
    {
        private Dictionary<int, MatchInfo> matches = new Dictionary<int, MatchInfo>();
        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private byte currentMatchId = 0;

        private byte nextMatchId => ++currentMatchId;

        private void Awake ()
        {
            if (!NetworkManager.Instance.MyPlayerInfo.IsMasterClient)
            {
                Destroy(this);
                return;
            }
            NetworkManager.OnEventReceived += HandleEventReceived;

            List<PlayerInfo> list = NetworkManager.Instance.CurrentRoomInfo.Players.CloneList();
            for (int i = 0; i < list.Count; i++)
                players.Add(list[i].Id, list[i]);
        }

        private void OnDestroy ()
        {
            NetworkManager.OnEventReceived -= HandleEventReceived;
        }

        private void Start ()
        {
        }

        private void HandleEventReceived (string key, object[] parameters)
        {
        }

        private void PrepareTournament ()
        {
            TournamentInfo tournament = new TournamentInfo();
            string[] keys = new string[players.Count];
            players.Keys.CopyTo(keys, 0);
            keys = keys.Shuffle();
            List<MatchInfo> matchList = new List<MatchInfo>();
            for (int i = 0; i < keys.Length; i += 2)
            {
                bool isByePlayer = i + 1 >= keys.Length;
                MatchInfo newMatch = GetNewMatch(players[keys[i]], isByePlayer ? null : players[keys[i + 1]]);
                matchList.Add(newMatch);
                matches.Add(newMatch.Id, newMatch);
            }
            tournament.Matches = matchList.ToArray();
            NetworkManager.Instance.ExecuteEvent(Keys.TournamentUpdateEvt, Keys.TournamentInfoKey, JsonConvert.SerializeObject(tournament));
        }

        private MatchInfo GetNewMatch (PlayerInfo player1, PlayerInfo player2)
        {
            return new MatchInfo()
            {
                Id = nextMatchId,
                Player1 = player1,
                Player2 = player2,
                Player1Wins = 0,
                Player2Wins = 0
            };
        }
    }
}
