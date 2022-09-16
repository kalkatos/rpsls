using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;

namespace Kalkatos.Tournament
{

    public class GameManagerServer : MonoBehaviour
    {
        private RoundInfo currentTournament;
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

            StartCoroutine(GameLoop());
        }

        private void OnDestroy ()
        {
            NetworkManager.OnEventReceived -= HandleEventReceived;
            NetworkManager.OnRequestDataSuccess -= HandleRequestDataSuccess;
        }

        private IEnumerator GameLoop ()
        {
            yield return WaitUntil(() => isConnectedAndInRoom);
            GetPlayers();
            PrepareTournament();
            yield return WaitClientsState(ClientState.GameReady);
            SendTournament();
            yield return WaitClientsState(ClientState.MatchReady);
            Debug.Log("Everyone ready for match!");
        }

        private IEnumerator WaitUntil (Func<bool> condition)
        {
            while (!condition.Invoke())
                yield return wait;
        }

        private IEnumerator WaitClientsState (ClientState expectedState)
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
            
        }

        private void GetPlayers ()
        {
            List<PlayerInfo> list = NetworkManager.Instance.CurrentRoomInfo.Players.CloneList();
            for (int i = 0; i < list.Count; i++)
            {
                players.Add(list[i].Id, list[i]); 
                if (list[i].IsBot)
                {
                    BotClient bot = gameObject.AddComponent<BotClient>();
                    bot.SetInfo(list[i]);
                }
            }
        }

        private void PrepareTournament ()
        {
            currentTournament = new RoundInfo();
            string[] keys = new string[players.Count];
            players.Keys.CopyTo(keys, 0);
            keys = keys.Shuffle();
            List<MatchInfo> matchList = new List<MatchInfo>();
            for (int i = 0; i < keys.Length; i += 2)
            {
                bool isByePlayer = i + 1 >= keys.Length;
                PlayerInfo p1 = players[keys[i]];
                PlayerInfo p2 = isByePlayer ? null : players[keys[i + 1]];
                p1.CustomData.UpdateOrAdd(new Dictionary<string, object>() { { Keys.IsByeKey, isByePlayer } });
                NetworkManager.Instance.UpdatePlayerCustomData(p1.Id, "MasterKey", "", p1.CustomData.ToObjArray());
                if (p2 != null)
                {
                    p2.CustomData.UpdateOrAdd(new Dictionary<string, object>() { { Keys.IsByeKey, false } });
                    NetworkManager.Instance.UpdatePlayerCustomData(p2.Id, "MasterKey", "", p2.CustomData.ToObjArray());
                }
                MatchInfo newMatch = GetNewMatch(p1, p2);
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
