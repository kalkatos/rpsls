using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Kalkatos.Tournament;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ClientState = Kalkatos.Tournament.ClientState;

namespace Kalkatos.Rpsls
{
    [DefaultExecutionOrder(-1)]
    public class GameManagerServer : MonoBehaviourPunCallbacks
    {
        [SerializeField] private bool debug;
        [SerializeField] private int botAmount;
        [SerializeField] private GameManager gameManager;

        private TournamentGameSettings settings;
        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private Dictionary<string, Player> playersInPhoton = new Dictionary<string, Player>();
        private TournamentInfo currentTournament;
        private RoundInfo currentRound;
        private byte currentTurn = 0;
        private List<Tuple<string, string>> clientsChecked = new List<Tuple<string, string>>();
        private WaitForSeconds delayToCheckClients = new WaitForSeconds(0.5f);
        private WaitForSeconds wait = new WaitForSeconds(0.5f);
        private bool becameNewMaster;
        private bool hasJoinedLobby;

        private const string tournamentOverKey = "TOver";
        private const string matchOverKey = "MOver";
        private const string turnOverKey = "UOver";
        private const string expectedStateKey = "ExSte";

        private int currentRoundIndex => currentTournament.Rounds.Length - 1;
        private bool tournamentIsOver => IsTrue(tournamentOverKey);
        private bool matchIsOver => IsTrue(matchOverKey);
        private bool turnIsOver => IsTrue(turnOverKey);
        private bool isConnectedAndInRoom => NetworkManager.Instance.IsConnected && NetworkManager.Instance.IsInRoom;
        private List<PlayerInfo> playersList
        {
            get
            {
                var list = new List<PlayerInfo>();
                foreach (var item in players)
                    list.Add(item.Value);
                return list;
            }
        }
        private string currentExpectedState
        {
            get
            {
                if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(expectedStateKey, out object value))
                    return value.ToString();
                return ClientState.Undefined;
            }
            set => PhotonNetwork.CurrentRoom?.SetCustomProperties(new Hashtable() { { expectedStateKey, value } });
        }

        private void Awake ()
        {
            settings = TournamentGameSettings.Instance;
            if (debug)
                StartCoroutine(ConnectForDebug());
            else if (NetworkManager.Instance.MyPlayerInfo.IsMasterClient)
            {
                currentExpectedState = ClientState.Undefined;
                StartCoroutine(GameLoop());
            }
        }

        private IEnumerator ConnectForDebug ()
        {
            DestroyImmediate(gameManager);
            PhotonNetwork.ConnectUsingSettings();
            while (!hasJoinedLobby)
                yield return null;
            PhotonNetwork.JoinOrCreateRoom("DEBUG", new Photon.Realtime.RoomOptions(), TypedLobby.Default);
            while (!PhotonNetwork.InRoom)
                yield return null;
            for (int i = 0; i < botAmount; i++)
                NetworkManager.Instance.AddBot();
            PhotonNetwork.LocalPlayer.NickName = "Kalkatos";
            GetPlayers(true);
            gameManager = gameObject.AddComponent<GameManager>();
            currentExpectedState = ClientState.Undefined;
            StartCoroutine(GameLoop());
        }

        private IEnumerator GameLoop ()
        {
            // Wait until connected and in a room
            yield return WaitUntil(() => isConnectedAndInRoom);
            Player[] photonPlayers = PhotonNetwork.PlayerList;
            for (int i = 0; i < photonPlayers.Length; i++)
                playersInPhoton.Add(photonPlayers[i].UserId, photonPlayers[i]);
            // If this is a fresh start, prepare the tournament. Just get the players otherwise.
            if (!becameNewMaster)
            {
                if (!debug)
                    GetPlayers(true);
                PrepareTournament();
                yield return WaitClientsState(ClientState.InGame);
            }
            else
                GetPlayers();
            // Begin the tournament - many matches
            while (!tournamentIsOver)
            {
                currentRound = currentTournament.Rounds[currentRoundIndex];
                if (!becameNewMaster)
                {
                    SendRound();
                    this.Log("Round: " + (currentRoundIndex + 1)); 
                }
                if (currentExpectedState == ClientState.InGame)
                    yield return WaitClientsState(ClientState.InMatch);
                this.Log("Everyone in match");
                // Go to match - many turns
                while (!matchIsOver)
                {
                    GetPlayers();
                    if (currentExpectedState == ClientState.InMatch)
                        yield return WaitClientsState(ClientState.InTurn);
                    currentTurn++;
                    this.Log("Everyone in turn " + currentTurn);
                    SendHand();
                    if (currentExpectedState == ClientState.InTurn)
                        yield return WaitClientsState(ClientState.HandReceived);
                    this.Log("Everyone got their hands");
                    yield return new WaitForSeconds(settings.TurnDuration);
                    if (currentExpectedState == ClientState.HandReceived)
                        yield return WaitClientsState(ClientState.WaitingTurnResult);
                    SendTurnResult();
                    this.Log("End of turn");
                    yield return null;
                }
                // End of match routines
                currentTurn = 0;
                SetFalse(matchOverKey);
                becameNewMaster = false;
                yield return null;
            }
        }

        private IEnumerator WaitUntil (Func<bool> condition)
        {
            while (!condition.Invoke())
                yield return wait;
        }

        private IEnumerator WaitClientsState (string expectedState)
        {
            currentExpectedState = expectedState;
            clientsChecked.Clear();
            while (players.Count > clientsChecked.Count)
            {
                yield return delayToCheckClients;
                GetPlayers();
                foreach (var item in players)
                    if (item.Value.CustomData.TryGetValue(Keys.PlayerStatusKey, out object state))
                        clientsChecked.Add(new Tuple<string, string>(item.Value.Id, state.ToString()));
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

        private bool IsTrue (string key)
        {
            return PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value) && bool.Parse(value.ToString());
        }

        private void SetFalse (string key)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { key, "false" } });
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
            currentTournament = FunctionInvoker.CreateTournament(playersList.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { Keys.TournamentsKey, JsonConvert.SerializeObject(currentTournament) } });
        }

        private void SendRound ()
        {
            NetworkManager.Instance.BroadcastEvent(Keys.RoundReceivedEvt, Keys.RoundKey, JsonConvert.SerializeObject(currentRound));
        }

        private void SendHand ()
        {
            this.Log("Sending match state.");
            NetworkManager.Instance.BroadcastEvent(Keys.HandReceivedEvt, Keys.HandKey, "");
        }

        private void SendTurnResult ()
        {
            foreach (var item in currentRound.Matches)
            {
                if (item.Player2 == null)
                    continue;
                // TODO Do proper results
                int randomWinner = UnityEngine.Random.Range(0, 2);
                if (randomWinner == 0)
                    item.Player1Wins++;
                else
                    item.Player2Wins++;
                // Save results in Round
                string p1NewMatchRecord = $"{item.Player1Wins}-{item.Player2Wins}";
                string p2NewMatchRecord = $"{item.Player2Wins}-{item.Player1Wins}";
                item.Player1.CustomData = item.Player1.CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, p1NewMatchRecord);
                item.Player2.CustomData = item.Player2.CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, p2NewMatchRecord);
                foreach (var item2 in PhotonNetwork.CurrentRoom.Players)
                {
                    if (item2.Value.UserId == item.Player1.Id)
                        item2.Value.SetCustomProperties(new Hashtable { { Keys.MatchRecordKey, p1NewMatchRecord } });
                    else if (item2.Value.UserId == item.Player2.Id)
                        item2.Value.SetCustomProperties(new Hashtable { { Keys.MatchRecordKey, p2NewMatchRecord } });
                }
            }
            // Save tournament
            currentTournament.Rounds[currentRound.Index] = currentRound;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { Keys.TournamentsKey, JsonConvert.SerializeObject(currentTournament) } });
            // Send updated Turn
            NetworkManager.Instance.BroadcastEvent(Keys.TurnEndedEvt, Keys.RoundKey, JsonConvert.SerializeObject(currentRound));
        }

        #region ========== Callbacks ===============

        public override void OnJoinedLobby ()
        {
            hasJoinedLobby = true;
        }

        public override void OnMasterClientSwitched (Player newMasterClient)
        {
            if (newMasterClient.IsLocal)
            {
                becameNewMaster = true;
                StartCoroutine(GameLoop());
            }
        }

        #endregion
    }

    public interface IGame
    {
        public void OnMatchStarted(MatchInfo match);
        public void OnMatchEnded(MatchInfo match);
        public void OnTurnStarted(MatchInfo match, int turnNumber);
        public void OnTurnEnded(MatchInfo match, int turnNumber);
        public void OnPhaseStarted(MatchInfo match, string phaseName);
        public void OnPhaseEndted(MatchInfo match, string phaseName);
        public void OnStepStarted(MatchInfo match, string stepName);
        public void OnStepEnded(MatchInfo match, string stepName);
    }
}
