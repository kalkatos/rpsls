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
        private List<string> pingResponsePlayers = new List<string>();
        private WaitForSeconds pingWait = new WaitForSeconds(0.5f);
        private byte currentMatchId = 0;
        private byte currentPingId = 0;
        private bool hasPingPassed;

        private byte nextMatchId => ++currentMatchId;
        private byte nextPingId
        {
            get
            {
                currentPingId = (byte)((currentPingId + 1) % byte.MaxValue); 
                return currentPingId; 
            }
        }

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
            PingAndWait(PrepareTournament);  
        }

        private void PingAndWait (Action callback)
        {
            NetworkManager.Instance.ExecuteEvent(Keys.ServerPing, Keys.PingSentId, nextPingId);
            StartCoroutine(PingAndWaitCoroutine(callback));
        }

        private IEnumerator PingAndWaitCoroutine (Action callback)
        {
            hasPingPassed = false;
            while (!hasPingPassed)
            {
                yield return pingWait;
            }
            callback.Invoke();
        }

        private void HandleEventReceived (string key, object[] parameters)
        {
            var paramDict = parameters.ToDictionary();
            if (key == Keys.ResponsePing)
            {
                if (paramDict.TryGetValue(Keys.PingRespondantInfo, out object respondant) && paramDict.TryGetValue(Keys.PingSentId, out object pingId))
                {
                    string respondantId = respondant.ToString();
                    if (players.ContainsKey(respondantId) && currentPingId == byte.Parse(pingId.ToString()))
                    {
                        pingResponsePlayers.Add(respondantId);
                        if (pingResponsePlayers.Count == players.Count)
                        {
                            pingResponsePlayers.Clear();
                            PingPassed();
                        }
                    }
                }
            }
        }

        private void PingPassed ()
        {
            this.Log("Ping has passed: " + currentPingId);
            hasPingPassed = true;
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
            NetworkManager.Instance.ExecuteEvent(Keys.TournamentUpdate, Keys.TournamentInfo, JsonConvert.SerializeObject(tournament));
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
