using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Kalkatos.Tournament
{
    [DefaultExecutionOrder(-1)]
    public class GameManagerServer : MonoBehaviourPunCallbacks
    {
        [SerializeField] private bool debug;
        [SerializeField] private GameManager gameManager;

        private Dictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private TournamentInfo currentTournament;
        private RoundInfo currentRound;
        private byte currentMatchId = 0;
        private byte currentTurn = 0;
        private List<Tuple<string, ClientState>> clientsChecked = new List<Tuple<string, ClientState>>();
        private WaitForSeconds delayToCheckClients = new WaitForSeconds(0.5f);
        private WaitForSeconds wait = new WaitForSeconds(0.5f);
        private bool becameNewMaster;
        private bool hasJoinedLobby;

        private const string isOverKey = "Over";
        private const string expectedStateKey = "ExSte";

        private int currentRoundIndex => currentTournament.Rounds.Length - 1;
        private bool isOver => HasData(isOverKey);
        private byte nextMatchId => ++currentMatchId;
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
        private ClientState currentExpectedState
        {
            get
            {
                if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(expectedStateKey, out object value))
                    return (ClientState)int.Parse(value.ToString());
                return ClientState.Undefined;
            }
            set => PhotonNetwork.CurrentRoom?.SetCustomProperties(new Hashtable() { { expectedStateKey, (int)value } });
        }

        private void Awake ()
        {
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
            // If this is a fresh start, prepare the tournament. Just get the players otherwise.
            if (!becameNewMaster)
            {
                if (!debug)
                    GetPlayers(true);
                PrepareTournament();
                currentExpectedState = ClientState.GameReady;
                yield return WaitClientsState();
            }
            else
                GetPlayers(false);
            // Begin the loop
            while (!isOver)
            {
                currentRound = currentTournament.Rounds[currentRoundIndex];
                if (!becameNewMaster)
                    SendRound();
                if (currentExpectedState == ClientState.GameReady)
                {
                    currentExpectedState = ClientState.MatchReady;
                    yield return WaitClientsState();
                }
                if (currentExpectedState == ClientState.MatchReady)
                {
                    currentExpectedState = ClientState.TurnReady;
                    Debug.Log("Round: " + (currentRoundIndex + 1));
                    yield return new WaitForSeconds(1f);
                    GetPlayers(false);
                    foreach (var item in players)
                    {

                    }
                }
                becameNewMaster = false;
            }
        }

        private IEnumerator WaitUntil (Func<bool> condition)
        {
            while (!condition.Invoke())
                yield return wait;
        }

        private IEnumerator WaitClientsState ()
        {
            clientsChecked.Clear();
            while (players.Count > clientsChecked.Count)
            {
                yield return delayToCheckClients;
                object[] playerIds = new object[players.Count];
                int index = 0;
                foreach (var item in players)
                {
                    playerIds[index] = $"{Keys.PlayerStatusKey}-{item.Key}";
                    index++;
                }
                // TODO Change to get directly
                NetworkManager.Instance.RequestCustomData(playerIds);
                while (clientsChecked.Count == 0)
                    yield return null;
                if (players.Count > clientsChecked.Count)
                    continue;
                //TODO Check disconnected players to get out of this loop
                int correctStateCount = 0;
                for (int i = 0; i < clientsChecked.Count; i++)
                    if (clientsChecked[i].Item2 == currentExpectedState)
                        correctStateCount++;
                if (correctStateCount == players.Count)
                    break;
            }
        }

        private bool HasData (string key)
        {
            return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key);
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
            NetworkManager.Instance.BroadcastEvent(Keys.RoundReceivedEvt, Keys.TournamentIdKey, JsonConvert.SerializeObject(currentRound));
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
}
