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
        private byte currentTurn = 0;
        private HashSet<string> clientsChecked = new HashSet<string>();
        private WaitForSeconds delayToCheckClients = new WaitForSeconds(0.5f);
        private WaitForSeconds wait = new WaitForSeconds(0.5f);
        private bool becameNewMaster;
        private bool hasJoinedLobby;
        private bool hasJoinedRoom;
        private bool tournamentIsOverLocal;
        private bool roundIsOverLocal;
        private bool turnIsOverLocal;

        private const string tournamentOverKey = "TOver";
        private const string roundOverKey = "ROver";
        private const string turnOverKey = "UOver";
        private const string expectedStateKey = "ExSte";

        private RoundInfo currentRound => currentTournament.Rounds[currentRoundIndex];
        private int currentRoundIndex => currentTournament.Rounds.Length - 1;
        private bool tournamentIsOver
        {
            get
            {
                return tournamentIsOverLocal;
            }
            set
            {
                tournamentIsOverLocal = value;
                SetBool(tournamentOverKey, value);
            }
        }
        private bool roundIsOver
        {
            get
            {
                return roundIsOverLocal;
            }
            set
            {
                roundIsOverLocal = value;
                SetBool(roundOverKey, value);
            }
        }
        private bool turnIsOver
        {
            get
            {
                return turnIsOverLocal;
            }
            set
            {
                turnIsOverLocal = value;
                SetBool(turnOverKey, value);
            }
        }
        private bool isConnectedAndInRoom => NetworkManager.Instance.IsConnected && NetworkManager.Instance.IsInRoom;
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
            while (!hasJoinedRoom)
                yield return null;
            for (int i = 0; i < botAmount; i++)
            {
                PlayerInfo botInfo = NetworkManager.Instance.AddBot();
                BotClient bot = gameObject.AddComponent<BotClient>();
                bot.SetId(botInfo.Id);
            }
            PhotonNetwork.LocalPlayer.NickName = "Kalkatos";
            GetPlayers();
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
                GetPlayers();
                PrepareTournament();
                yield return WaitClientsState(ClientState.InGame);
            }
            else
            {
                GetPlayers();
                // TODO Get tournament in room custom properties
            }
            // Begin the tournament - many matches
            while (!tournamentIsOver)
            {
                if (!becameNewMaster)
                {
                    SendRound();
                    this.Log("Round: " + (currentRoundIndex + 1));
                }
                yield return WaitClientsState(ClientState.InMatch);
                this.Log("Everyone in match");
                // Go to match - many turns
                while (!roundIsOver)
                {
                    GetPlayers();
                    yield return WaitClientsState(ClientState.InTurn);
                    currentTurn++;
                    this.Log("Everyone in turn " + currentTurn);
                    SendHand();
                    yield return WaitClientsState(ClientState.HandReceived);
                    this.Log("Everyone got their hands");
                    yield return new WaitForSeconds(settings.TurnDuration);
                    yield return WaitClientsState(ClientState.WaitingTurnResult);
                    SendTurnResult();
                    this.Log("End of turn");
                    yield return null;
                }
                // End of match routines
                this.Log("End of Round");
                currentTurn = 0;
                roundIsOver = false;
                becameNewMaster = false;
                AdvanceTournament();
                yield return WaitClientsState(ClientState.BetweenRounds);
                yield return null;
            }
            this.Log("End of tournament");
        }

        private IEnumerator WaitUntil (Func<bool> condition)
        {
            while (!condition.Invoke())
                yield return wait;
        }

        private IEnumerator WaitClientsState (string expectedState)
        {
            currentExpectedState = expectedState;
            GetPlayers();
            clientsChecked.Clear();
            this.Log("Waiting for clients state: " + expectedState);
            while (players.Count > clientsChecked.Count)
            {
                yield return delayToCheckClients;
                GetPlayers();
                //Hashtable data = PhotonNetwork.CurrentRoom.CustomProperties;

                foreach (var item in players)
                {
                    PlayerInfo player = item.Value;
                    if (player.CustomData.TryGetValue(Keys.ClientStateKey, out object state)
                        && state.ToString() == expectedState
                        && !clientsChecked.Contains(player.Id))
                    {
                        clientsChecked.Add(player.Id);
                    }
                }
            }
            this.Log("All clients in state: " + expectedState);
        }

        private void SetBool (string key, bool value)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { key, value } });
        }

        private void GetPlayers ()
        {
            ((PhotonNetworkManager)NetworkManager.Instance).UpdatePlayerList();
            players.Clear();
            var list = NetworkManager.Instance.Players;
            for (int i = 0; i < list.Length; i++)
                players.Add(list[i].Id, list[i]);
        }

        private void PrepareTournament ()
        {
            currentTournament = FunctionInvoker.CreateTournament(NetworkManager.Instance.Players);
            UpdatePhotonPlayers();
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

        private void UpdatePhotonPlayers ()
        {
            foreach (var item in PhotonNetwork.CurrentRoom.Players)
            {
                item.Value.SetCustomProperties(players[item.Value.UserId].CustomData.ToHashtable());
            }
        }

        private void SendTurnResult ()
        {
            GetPlayers();
            int endedMatches = 0;
            foreach (var item in currentRound.Matches)
            {
                if (item.IsOver)
                {
                    endedMatches++;
                    continue;
                }

                // TODO  >>>>  Do proper results <<<<<<
                int randomWinner = UnityEngine.Random.Range(0, 2);
                if (randomWinner == 0)
                    item.Player1Wins++;
                else
                    item.Player2Wins++;

                // Save results in Round
                item.IsOver = item.Player1Wins >= settings.TurnVictories || item.Player2Wins >= settings.TurnVictories;
                if (item.IsOver)
                    endedMatches++;
                // Save results in players
                string p1NewMatchRecord = $"{item.Player1Wins}-{item.Player2Wins}";
                string p2NewMatchRecord = $"{item.Player2Wins}-{item.Player1Wins}";
                players[item.Player1].CustomData = players[item.Player1].CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, p1NewMatchRecord);
                players[item.Player2].CustomData = players[item.Player2].CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, p2NewMatchRecord);
            }
            UpdatePhotonPlayers();
            if (endedMatches >= currentRound.Matches.Length)
            {
                roundIsOver = true;
                currentRound.IsOver = true;
                FunctionInvoker.EndRound(currentTournament, NetworkManager.Instance.Players);
            }
            // Save tournament
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { Keys.TournamentsKey, JsonConvert.SerializeObject(currentTournament) } });
            // Send updated Turn
            NetworkManager.Instance.BroadcastEvent(Keys.TurnEndedEvt, Keys.RoundKey, JsonConvert.SerializeObject(currentRound));
        }

        private void AdvanceTournament ()
        {
            string log = FunctionInvoker.AdvanceTournament(currentTournament, NetworkManager.Instance.Players, debug);
            if (debug)
                this.Log(" ====> Advance Tournament Log: " + log);
            UpdatePhotonPlayers();
        }

        #region ========== Callbacks ===============

        public override void OnJoinedLobby ()
        {
            hasJoinedLobby = true;
        }

        public override void OnJoinedRoom ()
        {
            hasJoinedRoom = true;
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
        public void OnMatchStarted (MatchInfo match);
        public void OnMatchEnded (MatchInfo match);
        public void OnTurnStarted (MatchInfo match, int turnNumber);
        public void OnTurnEnded (MatchInfo match, int turnNumber);
        public void OnPhaseStarted (MatchInfo match, string phaseName);
        public void OnPhaseEndted (MatchInfo match, string phaseName);
        public void OnStepStarted (MatchInfo match, string stepName);
        public void OnStepEnded (MatchInfo match, string stepName);
    }
}
