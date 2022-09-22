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
        private TournamentInfo currentTournament;
        private RoundInfo currentRound;
        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private byte currentMatchId = 0;
        private byte currentRoundIndex = 0;
        private byte currentTurn = 0;
        private List<Tuple<string, ClientState>> clientsChecked = new List<Tuple<string, ClientState>>();
        private WaitForSeconds delayToCheckClients = new WaitForSeconds(0.5f);
        private WaitForSeconds wait = new WaitForSeconds(0.5f);
        private bool isOver;

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
            GetPlayers(true);
            PrepareTournament();
            yield return WaitClientsState(ClientState.GameReady);
            while (!isOver)
            {
                currentRound = currentTournament.Rounds[currentRoundIndex];
                SendRound();
                yield return WaitClientsState(ClientState.MatchReady);
                Debug.Log("Round: " + (currentRoundIndex + 1));
                yield return new WaitForSeconds(1f);
                GetPlayers();
                for (int i = 0; i < players.Count; i++)
                {
                    // TODO Finish
                }
            }
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
                    playerIds[index] = $"{Keys.PlayerIdKey}-{item.Key}";
                    index++;
                }
                NetworkManager.Instance.RequestCustomData(playerIds);
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
                if (paramDict.TryGetValue($"{Keys.PlayerIdKey}-{item.Key}", out object state))
                {
                    if (state != null)
                        clientsChecked.Add(new Tuple<string, ClientState>(item.Key, (ClientState)int.Parse(state.ToString())));
                }
        }

        private void HandleEventReceived (string key, object[] parameters)
        {

        }

        private void GetPlayers (bool createBots = false)
        {
            List<PlayerInfo> list = NetworkManager.Instance.CurrentRoomInfo.Players.CloneList();
            players.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                players.Add(list[i].Id, list[i]);
                if (createBots && list[i].IsBot)
                {
                    BotClient bot = gameObject.AddComponent<BotClient>();
                    bot.SetInfo(list[i]);
                }
            }
        }

        private void PrepareTournament ()
        {
            FunctionInvoker.ExecuteFunction(nameof(FunctionInvoker.StartTournament), NetworkManager.Instance.CurrentRoomInfo.Id,
                (success) =>
                {
                    currentTournament = (TournamentInfo)success;
                    NetworkManager.Instance.SendCustomData(Keys.TournamentIdKey, currentTournament);
                },
                (failure) =>
                {
                    this.LogError(failure.ToString());
                });
        }

        private void SendRound ()
        {
            this.Log("Tournament:  " + JsonConvert.SerializeObject(currentTournament, Formatting.Indented));
            NetworkManager.Instance.BroadcastEvent(Keys.TournamentUpdateEvt, Keys.TournamentIdKey, JsonConvert.SerializeObject(currentRound, Formatting.Indented));
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
