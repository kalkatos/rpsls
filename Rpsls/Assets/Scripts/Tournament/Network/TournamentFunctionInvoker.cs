using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Kalkatos.Tournament;

namespace Kalkatos.Network
{
    public partial class FunctionInvoker
    {
        public static object Foo (object[] parameters)
        {
            return parameters[0];
        }

        public static RoundInfo CreateRound (PlayerInfo[] players, int index)
        {
            MatchInfo[] matches = new MatchInfo[(int)Mathf.Ceil(players.Length / 2f)];
            for (int i = 0; i < players.Length; i += 2)
            {
                bool isBye = i == players.Length - 1;
                PlayerInfo p1 = players[i];
                PlayerInfo p2 = isBye ? null : players[i + 1];
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(Keys.IsByeKey, isBye);
                dict.Add(Keys.MatchRecordKey, "0-0");
                if (!p1.CustomData.ContainsKey(Keys.TournamentRecordKey))
                    dict.Add(Keys.TournamentRecordKey, "0-0");
                p1.CustomData = p1.CustomData.CloneWithUpdateOrAdd(dict);
                if (!isBye)
                {
                    dict.Clear();
                    dict.Add(Keys.IsByeKey, false);
                    dict.Add(Keys.MatchRecordKey, "0-0");
                    if (!p2.CustomData.ContainsKey(Keys.TournamentRecordKey))
                        dict.Add(Keys.TournamentRecordKey, "0-0");
                    p2.CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(dict);
                }
                int matchIndex = (int)Mathf.Ceil(i / 2f);
                matches[matchIndex] = new MatchInfo()
                {
                    MatchId = Guid.NewGuid().ToString(),
                    Player1 = p1.Id,
                    Player2 = isBye ? "" : p2.Id,
                    Player1Wins = 0,
                    Player2Wins = 0,
                    IsOver = isBye
                };
            }
            return new RoundInfo()
            {
                RoundId = Guid.NewGuid().ToString(),
                Index = index,
                Matches = matches
            };
        }

        private static int[] ExtractRecord (PlayerInfo player)
        {
            string[] recordSplit = player.CustomData[Keys.TournamentRecordKey].ToString().Split('-');
            return new int[] { int.Parse(recordSplit[0]), int.Parse(recordSplit[1]) };
        }

        private static void UpdatePlayerRecord (PlayerInfo player, bool isVictorious)
        {
            int[] records = ExtractRecord(player);
            string newRecord = isVictorious ? $"{records[0] + 1}-{records[1]}" : $"{records[0]}-{records[1] + 1}";
            player.CustomData = player.CustomData.CloneWithUpdateOrAdd(Keys.TournamentRecordKey, newRecord);
        }

        public static TournamentInfo CreateTournament (PlayerInfo[] players, bool addZeroedRecords = true)
        {
            players.Shuffle();
            string[] playerIds = new string[players.Length];
            for (int i = 0; i < players.Length; i++)
                playerIds[i] = players[i].Id;
            TournamentInfo tournament = new TournamentInfo();
            tournament.TournamentId = Guid.NewGuid().ToString();
            tournament.Players = playerIds;
            tournament.Rounds = new RoundInfo[] { CreateRound(players, 0) };
            return tournament;
        }

        private static Dictionary<string, PlayerMetadata> ExtractMetadata (TournamentInfo tournament, PlayerInfo[] players)
        {
            Dictionary<string, PlayerMetadata> metadata = new Dictionary<string, PlayerMetadata>();
            foreach (var player in players)
            {
                PlayerMetadata playerMetadata = new PlayerMetadata();
                foreach (var round in tournament.Rounds)
                {
                    foreach (var match in round.Matches)
                    {
                        if (match.Player1 == player.Id)
                        {
                            playerMetadata.FacedOpponents.Add(match.Player2);
                            playerMetadata.HadByeRound |= string.IsNullOrEmpty(match.Player2);
                        }
                        else if (match.Player2 == player.Id)
                            playerMetadata.FacedOpponents.Add(match.Player1);
                    }
                }
                playerMetadata.Record = ExtractRecord(player);
                metadata.Add(player.Id, playerMetadata);
            }
            return metadata;
        }

        public static void EndRound (TournamentInfo fromTournament, PlayerInfo[] players)
        {
            // Finalize
            foreach (var item in players)
            {
                if (!item.CustomData.ContainsKey(Keys.TournamentRecordKey))
                    item.CustomData.Add(Keys.TournamentRecordKey, "0-0");
            }
            // Update records
            List<PlayerInfo> playersList = new List<PlayerInfo>();
            playersList.AddRange(players);
            foreach (var item in fromTournament.Rounds[fromTournament.Rounds.Length - 1].Matches)
            {
                PlayerInfo p1 = playersList.Find((p) => p.Id == item.Player1);
                PlayerInfo p2 = playersList.Find((p) => p.Id == item.Player2);
                bool isByeMatch = string.IsNullOrEmpty(item.Player2);
                UpdatePlayerRecord(p1, isByeMatch || item.Player1Wins > item.Player2Wins);
                if (p2 != null)
                    UpdatePlayerRecord(p2, item.Player2Wins > item.Player1Wins);
                if (!isByeMatch && item.Player1Wins == item.Player2Wins)
                    Debug.LogWarning($"Advance Tournament is not prepared to received Draws! Match between {p1?.Nickname} and {p2?.Nickname}");
            }
        }

        public static string AdvanceTournament (TournamentInfo fromTournament, PlayerInfo[] players, bool returnLog = false)
        {
            // Check tournament ended
            TournamentGameSettings settings = GetTournamentSettings();
            if (fromTournament.Rounds.Length == settings.GetNumberOfRounds(players.Length))
            {
                fromTournament.IsOver = true;
                return "";
            }

            List<object> logObjList = new List<object>();

            // DEBUG
            if (returnLog)
            {
                logObjList.Add("Tournament Before");
                logObjList.Add(new TournamentInfo(fromTournament));
                logObjList.Add("Players Before");
                logObjList.Add(Clone(players));
            }

            // Create metadata
            Dictionary<string, PlayerMetadata> metadata = ExtractMetadata(fromTournament, players);
            if (returnLog)
            {
                logObjList.Add("Metadata");
                logObjList.Add(metadata.ToObjArray());
            }

            // Shuffle
            players.Shuffle();

            // Get bye first and/or sort
            if (players.Length % 2 == 1)
            {
                // Go through the list of random players to find one that has not had bye before
                for (int i = 0; i < players.Length; i++)
                {
                    if (!metadata[players[i].Id].HadByeRound)
                    {
                        ExchangePlayersPositions(i, players.Length - 1);
                        break;
                    }
                }
                SortPlayersByRanking(players, 0, players.Length - 1);
            }
            else
                SortPlayersByRanking(players);

            // Order players for new matches
            for (int i = 0; i < players.Length - 1; i += 2)
            {
                PlayerInfo p1 = players[i];
                PlayerInfo p2 = players[i + 1];
                if (metadata[p1.Id].FacedOpponents.Contains(p2.Id))
                {
                    if (returnLog) logObjList.Add($"{p1.Nickname} has already faced {p2.Nickname}, changing.");
                    bool foundReplacement = false;
                    for (int j = 0; j < players.Length; j++)
                    {
                        if (players[j] == p1 || players[j] == p2)
                            continue;
                        if (!metadata[p1.Id].FacedOpponents.Contains(players[j].Id)
                            && (metadata[players[j].Id].Record[0] == metadata[p2.Id].Record[0] 
                                || metadata[players[j].Id].Record[0] == metadata[p1.Id].Record[0]))
                        {
                            if (returnLog) logObjList.Add($"{p2.Nickname} replaced by {players[j].Nickname}.");
                            ExchangePlayersPositions(j, i + 1);
                            foundReplacement = true;
                            break;
                        }
                    }
                    if (!foundReplacement)
                        if (returnLog) logObjList.Add($"Didn't find a replacement for {p1.Nickname} vs {p2.Nickname}.");
                }
            }

            // Return to players
            int playersIndex = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                    continue;
                players[playersIndex] = players[i];
                playersIndex++;
            }

            // Add new round
            fromTournament.Rounds = fromTournament.Rounds.CloneWithAdd(CreateRound(players, fromTournament.Rounds.Length));

            if (returnLog)
            {
                logObjList.Add("Players and Tournament");
                logObjList.Add(new object[] { players, fromTournament });

                string log = JsonConvert.SerializeObject(logObjList, Formatting.Indented);
                Debug.Log(log);
                return log;
            }
            return "";

            void ExchangePlayersPositions (int a, int b)
            {
                PlayerInfo temp = players[a];
                players[a] = players[b];
                players[b] = temp;
            }

            PlayerInfo[] Clone (PlayerInfo[] players)
            {
                PlayerInfo[] playersClone = new PlayerInfo[players.Length];
                for (int i = 0; i < players.Length; i++)
                    if (players[i] != null)
                        playersClone[i] = new PlayerInfo(players[i]);
                return playersClone;
            }
        }

        public static void SortPlayersByRanking (PlayerInfo[] players, int index = 0, int length = -1)
        {
            if (length < 0)
                length = players.Length;
            Array.Sort(players, index, length, new PlayerInfoComparer());
        }

        public static TournamentGameSettings GetTournamentSettings ()
        {
            return TournamentGameSettings.Instance;
        }

        public static async Task<object> StartTournament (object roomId)
        {
            // Check if it's master
            if (!IsMasterClient)
                return Error.MustBeMaster;
            // Get Room
            object getRoomResult = await GetRoom(roomId);
            if (getRoomResult is Error)
                return getRoomResult;
            RoomInfo room = (RoomInfo)getRoomResult;
            // Get Players
            PlayerInfo[] players = NetworkManager.Instance.Players;
            // Create tournament
            TournamentInfo tournamentInfo = CreateTournament(players);
            // Update players
            foreach (var item in players)
                await DataAccess.SendData(item.Id, JsonConvert.SerializeObject(item, Formatting.Indented), Keys.ConnectedPlayersKey).ContinueWith((t) => { });
            // Add to room
            room.CustomData = room.CustomData.CloneWithUpdateOrAdd(Keys.TournamentsKey, tournamentInfo);
            await DataAccess.SendData(room.Id, JsonConvert.SerializeObject(room, Formatting.Indented), Keys.ActiveRoomsKey);
            await DataAccess.SendData(tournamentInfo.TournamentId, JsonConvert.SerializeObject(tournamentInfo, Formatting.Indented), Keys.TournamentsKey);
            return tournamentInfo;
        }

        public static async Task<object> GetRoom (object roomId)
        {
            // Check parameters
            if (roomId == null || !(roomId is string))
                return Error.WrongParameters;
            // Get Room
            string roomSerialized = (await GetData(roomId.ToString(), Keys.ActiveRoomsKey)).ToString();
            if (string.IsNullOrEmpty(roomSerialized))
                return Error.NotAvailable;
            RoomInfo room = JsonConvert.DeserializeObject<RoomInfo>(roomSerialized);
            return room;
        }

        // TODO GetTournamentFromRoom
        //public static async Task<object> GetTournamentFromRoom (object roomId)
        //{
        //    // Get Room
        //    object getRoomResult = await GetRoom(roomId);
        //    if (getRoomResult is Error)
        //        return getRoomResult;
        //    RoomInfo room = (RoomInfo)getRoomResult;
        //}

        /// <summary>
        /// Parameters: Tournament ID (string) ||
        /// Returns: TournamentInfo or Error
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static async Task<object> GetTournamentInfo (object parameter)
        {
            // Check parameters
            object[] parameters = (object[])parameter;
            var paramDict = parameters.ToDictionary();
            if (!paramDict.TryGetValue(Keys.TournamentIdKey, out object tournamentIdObj))
                return Error.WrongParameters;
            string tournamentId = tournamentIdObj.ToString();
            // Check if the tournament exists
            string tmtListSerialized = (await GetData(Keys.TournamentsKey)).ToString();
            if (string.IsNullOrEmpty(tmtListSerialized))
                return Error.NonExistent;
            Dictionary<string, object> tmtListDict = JsonConvert.DeserializeObject<object[]>(tmtListSerialized).ToDictionary();
            if (tmtListDict == null)
                return Error.NonExistent;
            object tournamentInfoObj;
            if (!tmtListDict.TryGetValue(tournamentId, out tournamentInfoObj))
                return Error.NotAvailable;
            // Check if this player is in that tournament
            TournamentInfo tournamentInfo = (TournamentInfo)tournamentInfoObj;
            string playerId = NetworkManager.Instance.MyPlayerInfo.Id;
            bool foundPlayer = false;
            for (int i = 0; i < tournamentInfo.Players.Length; i++)
                if (tournamentInfo.Players[i] == playerId)
                {
                    foundPlayer = true;
                    break;
                }
            if (!foundPlayer)
                return Error.NotAllowed;
            return tournamentInfo;
        }

        /// <summary>
        /// Parameters: Tournament ID (string), Round Number (int) ||
        /// Returns: RoundInfo or Error
        /// </summary>
        /// <param name="parameter">Tournament ID (string), Round Number (int)</param>
        /// <returns></returns>
        public static async Task<object> GetRoundInfo (object parameter)
        {
            object[] parameters = (object[])parameter;
            var paramDict = parameters.ToDictionary();
            if (!paramDict.TryGetValue(Keys.TournamentIdKey, out object tournamentIdObj) ||
                !paramDict.TryGetValue(Keys.RoundNumberKey, out object roundNumberObj))
                return Error.WrongParameters;
            object result = await GetTournamentInfo(parameter);
            if (result is Error)
                return result;
            TournamentInfo tournamentInfo = (TournamentInfo)result;
            int roundNumber = int.Parse(roundNumberObj.ToString());
            // Return round if it exists
            if (roundNumber < tournamentInfo.Rounds.Length)
                return tournamentInfo.Rounds[roundNumber];
            return Error.WrongParameters;
        }

        private class PlayerInfoComparer : IComparer<PlayerInfo>
        {
            public int Compare (PlayerInfo x, PlayerInfo y)
            {
                if (x == null || y == null)
                    return UnityEngine.Random.Range(-1, 2);
                int[] p1Record = ExtractRecord(x);
                int[] p2Record = ExtractRecord(y);
                return p2Record[0] - p1Record[0];
            }
        }

        private class PlayerMetadata
        {
            public bool HadByeRound;
            public int[] Record;
            public List<string> FacedOpponents = new List<string>();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tournament Test/End Round")]
        public static void TestEndRound ()
        {
            var stream = System.IO.File.ReadAllText(Application.dataPath + "/Test/tournament-test.json", System.Text.Encoding.UTF8);
            object[] data = JsonConvert.DeserializeObject<object[]>(stream);
            PlayerInfo[] players = JsonConvert.DeserializeObject<PlayerInfo[]>(data[0].ToString());
            TournamentInfo tournament = JsonConvert.DeserializeObject<TournamentInfo>(data[1].ToString());
            RoundInfo round = tournament.Rounds[tournament.Rounds.Length - 1];
            round.IsOver = true;
            foreach (var item in round.Matches)
            {
                if (item.IsOver)
                    continue;
                int randomWinner = UnityEngine.Random.Range(0, 2);
                if (randomWinner == 0)
                    item.Player1Wins = 3;
                if (randomWinner == 1)
                    item.Player2Wins = 3;
                item.IsOver = true;
            }
            EndRound(tournament, players);
            data[0] = players;
            data[1] = tournament;
            System.IO.File.WriteAllText(Application.dataPath + "/Test/tournament-test.json", JsonConvert.SerializeObject(data, Formatting.Indented), System.Text.Encoding.UTF8);
        }

        [UnityEditor.MenuItem("Tournament Test/Advance Tournament")]
        public static void TestAdvanceTournament ()
        {
            var stream = System.IO.File.ReadAllText(Application.dataPath + "/Test/tournament-test.json", System.Text.Encoding.UTF8);
            object[] data = JsonConvert.DeserializeObject<object[]>(stream);
            PlayerInfo[] players = JsonConvert.DeserializeObject<PlayerInfo[]>(data[0].ToString());
            TournamentInfo tournament = JsonConvert.DeserializeObject<TournamentInfo>(data[1].ToString());
            string log = AdvanceTournament(tournament, players, true);
            if (!string.IsNullOrEmpty(log))
                System.IO.File.WriteAllText(Application.dataPath + "/Test/tournament-test-result.json", log, System.Text.Encoding.UTF8);

            #region Tournament json example
            /*
            [
                [
                    {
                            "Id": "A",
                        "Nickname": "A",
                        "IsMasterClient": true,
                        "IsMe": true,
                        "IsBot": false,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "B",
                        "Nickname": "B",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "C",
                        "Nickname": "C",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "D",
                        "Nickname": "D",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "E",
                        "Nickname": "E",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "F",
                        "Nickname": "F",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "G",
                        "Nickname": "G",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "H",
                        "Nickname": "H",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": false,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        },
                    {
                            "Id": "I",
                        "Nickname": "I",
                        "IsMasterClient": false,
                        "IsMe": false,
                        "IsBot": true,
                        "CustomData": {
                                "IsBye": true,
                            "MRecd": "0-0",
                            "TRecd": "0-0",
                            "ClSts": "Waiting"
                        }
                        }
                ],
              {
                            "Id": "Tournament1",
                "IsOver": false,
                "Players": [
                  "A",
                  "B",
                  "C",
                  "D",
                  "E",
                  "F",
                  "G",
                  "H",
                  "I"
                ],
                "Rounds": [
                  {
                                "Id": "Round1",
                    "Index": 0,
                    "IsOver": true,
                    "Matches": [
                      {
                                    "Id": "1",
                        "Player1": "A",
                        "Player2": "B",
                        "Player1Wins": 3,
                        "Player2Wins": 0,
                        "IsOver": true
                      },
                      {
                                    "Id": "2",
                        "Player1": "C",
                        "Player2": "D",
                        "Player1Wins": 3,
                        "Player2Wins": 0,
                        "IsOver": true
                      },
                      {
                                    "Id": "3",
                        "Player1": "E",
                        "Player2": "F",
                        "Player1Wins": 3,
                        "Player2Wins": 0,
                        "IsOver": true
                      },
                      {
                                    "Id": "4",
                        "Player1": "G",
                        "Player2": "H",
                        "Player1Wins": 3,
                        "Player2Wins": 0,
                        "IsOver": true
                      },
                      {
                                    "Id": "5",
                        "Player1": "I",
                        "Player2": "",
                        "Player1Wins": 0,
                        "Player2Wins": 0,
                        "IsOver": true
                      }
                    ]
                  }
                ]
              }
            ]
            */
            #endregion
        }
#endif
    }
}
