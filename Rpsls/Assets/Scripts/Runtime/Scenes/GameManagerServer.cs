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
        private TournamentInfo currentTournament;
        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private byte currentMatchId = 0;
        private List<Tuple<string, ClientState>> clientsChecked = new List<Tuple<string, ClientState>>();
        private WaitForSeconds delayToCheckClients = new WaitForSeconds(0.5f);
        private WaitForSeconds wait = new WaitForSeconds(0.5f);

        private byte nextMatchId => ++currentMatchId;
        private bool isConnectedAndInRoom => NetworkManager.Instance.IsConnected && NetworkManager.Instance.IsInRoom;

        private void Awake ()
        {
            if (!NetworkManager.Instance.MyPlayerInfo.IsMasterClient)
            {
                Destroy(this);
                return;
            }
            NetworkManager.OnEventReceived += HandleEventReceived;
            NetworkManager.OnRequestDataSuccess += HandleRequestDataSuccess;

            WaitUntil(() => isConnectedAndInRoom, GetPlayers);
        }

        private void OnDestroy ()
        {
            NetworkManager.OnEventReceived -= HandleEventReceived;
            NetworkManager.OnRequestDataSuccess -= HandleRequestDataSuccess;
        }

        private void GetPlayers ()
        {
            List<PlayerInfo> list = NetworkManager.Instance.CurrentRoomInfo.Players.CloneList();
            for (int i = 0; i < list.Count; i++)
                players.Add(list[i].Id, list[i]);
            PrepareTournament();
            CheckClientsState(ClientState.GameReady, SendTournament);
        }

        private void WaitUntil (Func<bool> condition, Action callback)
        {
            StartCoroutine(WaitUntilCoroutine(condition, callback));
        }

        private IEnumerator WaitUntilCoroutine (Func<bool> condition, Action callback)
        {
            while (!condition.Invoke())
                yield return wait;
            callback?.Invoke();
        }

        private void CheckClientsState (ClientState expectedState, Action callback)
        {
            StartCoroutine(CheckClientsStateCoroutine(expectedState, callback));
        }

        private IEnumerator CheckClientsStateCoroutine (ClientState expectedState, Action callback)
        {
            clientsChecked.Clear();
            while (players.Count > clientsChecked.Count)
            {
                yield return delayToCheckClients;
                object[] playerIds = new object[players.Count];
                int index = 0;
                foreach (var item in players)
                {
                    playerIds[index] = $"{Keys.ClientIdKey}-{item.Key}";
                    index++;
                }
                NetworkManager.Instance.RequestData(playerIds);
                while (clientsChecked.Count == 0)
                    yield return null;
                if (players.Count > clientsChecked.Count)
                    continue;
                //TODO Check disconnected players to get out of this loop
                int correctStateCount = 0;
                for (int i = 0; i < clientsChecked.Count; i++)
                    if (clientsChecked[i].Item2 == expectedState)
                        correctStateCount++;
                if (correctStateCount == players.Count)
                    break;
            }
            callback?.Invoke();
        }

        private void HandleRequestDataSuccess (object[] parameters)
        {
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            foreach (var item in players)
                if (paramDict.TryGetValue($"{Keys.ClientIdKey}-{item.Key}", out object state))
                    clientsChecked.Add(new Tuple<string, ClientState>(item.Key, (ClientState)int.Parse(state.ToString())));
        }

        private void HandleEventReceived (string key, object[] parameters)
        {
            //TODO Receive other players ready
        }

        private void PrepareTournament ()
        {
            currentTournament = new TournamentInfo();
            string[] keys = new string[players.Count];
            players.Keys.CopyTo(keys, 0);
            keys = keys.Shuffle();
            List<MatchInfo> matchList = new List<MatchInfo>();
            for (int i = 0; i < keys.Length; i += 2)
            {
                bool isByePlayer = i + 1 >= keys.Length;
                MatchInfo newMatch = GetNewMatch(players[keys[i]], isByePlayer ? null : players[keys[i + 1]]);
                matchList.Add(newMatch);
            }
            currentTournament.Matches = matchList.ToArray();
        }

        private void SendTournament ()
        {
            NetworkManager.Instance.ExecuteEvent(Keys.TournamentUpdateEvt, Keys.TournamentInfoKey, JsonConvert.SerializeObject(currentTournament));
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
