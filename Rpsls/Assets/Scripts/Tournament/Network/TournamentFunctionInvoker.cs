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

        public static RoundInfo CreateRound (PlayerInfo[] players, bool addZeroedRecords)
        {
            MatchInfo[] matches = new MatchInfo[(int)Mathf.Ceil(players.Length / 2f)];
            for (int i = 0; i < players.Length; i += 2)
            {
                bool isBye = i == players.Length - 1;
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(Keys.IsByeKey, isBye);
                dict.Add(Keys.MatchRecordKey, "0-0");
                if (addZeroedRecords)
                    dict.Add(Keys.TournamentRecordKey, isBye ? "1-0" : "0-0");
                players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(dict);
                if (!isBye)
                {
                    dict.Clear();
                    dict.Add(Keys.IsByeKey, false);
                    dict.Add(Keys.MatchRecordKey, "0-0");
                    if (addZeroedRecords)
                        dict.Add(Keys.TournamentRecordKey, "0-0");
                    players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(dict);
                }
                int matchIndex = (int)Mathf.Ceil(i / 2f);
                matches[matchIndex] = new MatchInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    Player1 = players[i].Id,
                    Player2 = isBye ? "" : players[i + 1].Id,
                    Player1Wins = 0,
                    Player2Wins = 0,
                    IsOver = isBye
                };
            }
            return new RoundInfo()
            {
                Id = Guid.NewGuid().ToString(),
                Index = 0,
                Matches = matches
            };
        }

        public static TournamentInfo CreateTournament (PlayerInfo[] players, bool addZeroedRecords = true)
        {
            players.Shuffle();
            string[] playerIds = new string[players.Length];
            for (int i = 0; i < players.Length; i++)
                playerIds[i] = players[i].Id;
            TournamentInfo tournament = new TournamentInfo();
            tournament.Id = Guid.NewGuid().ToString();
            tournament.Players = playerIds;
            tournament.Rounds = new RoundInfo[] { CreateRound(players, addZeroedRecords) };
            return tournament;
        }

        public static TournamentInfo AdvanceTournament (TournamentInfo fromTournament, PlayerInfo[] players, ref string log)
        {
            List<object> logObjList = new List<object>();

            logObjList.Add("Tournament Before");
            logObjList.Add(new TournamentInfo(fromTournament));

            PlayerInfo[] Clone (PlayerInfo[] players)
            {
                PlayerInfo[] playersClone = new PlayerInfo[players.Length];
                for (int i = 0; i < players.Length; i++)
                    if (players[i] != null)
                        playersClone[i] = new PlayerInfo(players[i]);
                return playersClone;
            }
            logObjList.Add("Players Before");
            logObjList.Add(Clone(players));

            foreach (var item in players)
            {
                if (!item.CustomData.ContainsKey(Keys.TournamentRecordKey))
                    item.CustomData.Add(Keys.TournamentRecordKey, "0-0");
                if (!item.CustomData.ContainsKey(Keys.IsByeKey))
                    item.CustomData.Add(Keys.IsByeKey, false);
                else
                    item.CustomData[Keys.IsByeKey] = false;
            }
            List<PlayerInfo> playersList = new List<PlayerInfo>();
            playersList.AddRange(players);

            // Update records
            foreach (var item in fromTournament.Rounds[fromTournament.Rounds.Length - 1].Matches)
            {
                PlayerInfo p1 = playersList.Find((p) => p.Id == item.Player1);
                PlayerInfo p2 = playersList.Find((p) => p.Id == item.Player2);
                bool isByeMatch = p1 == null || p2 == null;
                if (!isByeMatch)
                {
                    UpdatePlayerRecord(p1, item.Player1Wins > item.Player2Wins);
                    UpdatePlayerRecord(p2, item.Player2Wins > item.Player1Wins);
                    if (item.Player1Wins == item.Player2Wins)
                        Debug.LogWarning($"Advance Tournament is not prepared to received Draws! Match between {p1?.Nickname} and {p2?.Nickname}");
                }
            }
            //log.Add("Updated Records");
            //log.Add(Clone(players));

            // Create metadata
            Dictionary<string, PlayerMetadata> metadata = new Dictionary<string, PlayerMetadata>();
            foreach (var player in playersList)
            {
                PlayerMetadata playerMetadata = new PlayerMetadata();
                foreach (var round in fromTournament.Rounds)
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
            logObjList.Add("Metadata");
            logObjList.Add(metadata.ToObjArray());

            // Just two players and they have already faced each other (as this functions should only be called after a match)
            if (players.Length == 2)
            {
                fromTournament.IsOver = true;
                return fromTournament;
            }

            // Shuffle
            players.Shuffle();

            // Get bye first
            if (players.Length % 2 == 1)
            {
                PlayerInfo byePlayer = null;
                // Go through the list of random players to find one that has not had bye before
                for (int i = 0; i < players.Length; i++)
                {
                    byePlayer = players[i];
                    if (!metadata[players[i].Id].HadByeRound)
                        break;
                }
                // Create and populate an array only with the other players
                PlayerInfo[] playersEven = new PlayerInfo[players.Length - 1];
                int indexWithoutBye = 0;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].Id == byePlayer.Id)
                        continue;
                    playersEven[indexWithoutBye] = players[i];
                    indexWithoutBye++;
                }
                // Sort the not-bye players and put them back on the players in the correct order with bye player last
                Array.Sort(playersEven, SortBasedOnRecord);
                for (int i = 0; i < playersEven.Length; i++)
                    players[i] = playersEven[i];
                players[players.Length - 1] = byePlayer;
            }
            else
                Array.Sort(players, SortBasedOnRecord);

            // Order players for new matches
            for (int i = 0; i < players.Length - 1; i += 2)
            {
                PlayerInfo p1 = players[i];
                PlayerInfo p2 = players[i + 1];
                if (metadata[p1.Id].FacedOpponents.Contains(p2.Id))
                {
                    bool foundNewWithSameRecord = false;
                    // Find an opponent that p1 has never faced before with the same record
                    for (int j = 0; j < players.Length; j++)
                    {
                        if (players[j] == p1 || players[j] == p2)
                            continue;
                        if (!metadata[p1.Id].FacedOpponents.Contains(players[j].Id)
                            && metadata[players[j].Id].Record[0] == metadata[p1.Id].Record[0])
                        {
                            ExchangePlayersPositions(j, i + 1);
                            foundNewWithSameRecord = true;
                            break;
                        }
                    }
                    if (!foundNewWithSameRecord)
                    {
                        // If there is no opponent with the same record, just find an opponent that has never faced before
                        for (int j = 0; j < players.Length; j++)
                        {
                            if (players[j] == p1 || players[j] == p2)
                                continue;
                            if (!metadata[p1.Id].FacedOpponents.Contains(players[j].Id))
                            {
                                ExchangePlayersPositions(j, i + 1);
                                break;
                            }
                        }
                    }
                }
            }

            //log.Add("Players Before Tournament");
            //log.Add(players);

            // Return to players
            int playersIndex = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                    continue;
                players[playersIndex] = players[i];
                playersIndex++;
            }

            //log.Add("Players Returned");
            //log.Add(Clone(players));

            // Add new round
            fromTournament.Rounds = fromTournament.Rounds.CloneWithAdd(CreateRound(players, false));

            void ExchangePlayersPositions (int a, int b)
            {
                PlayerInfo temp = players[a];
                players[a] = players[b];
                players[b] = temp;
            }

            int[] ExtractRecord (PlayerInfo player)
            {
                string[] recordSplit = player.CustomData[Keys.TournamentRecordKey].ToString().Split('-');
                return new int[] { int.Parse(recordSplit[0]), int.Parse(recordSplit[1]) };
            }

            void UpdatePlayerRecord (PlayerInfo player, bool isVictorious)
            {
                int[] records = ExtractRecord(player);
                string newRecord = isVictorious ? $"{records[0] + 1}-{records[1]}" : $"{records[0]}-{records[1] + 1}";
                player.CustomData = player.CustomData.CloneWithUpdateOrAdd(Keys.TournamentRecordKey, newRecord);
            }

            int SortBasedOnRecord (PlayerInfo p1, PlayerInfo p2)
            {
                if (p1 == null || p2 == null)
                    return UnityEngine.Random.Range(-1, 2);
                int[] p1Record = ExtractRecord(p1);
                int[] p2Record = ExtractRecord(p2);
                return p2Record[0] - p1Record[0];
            }

            logObjList.Add("Players and Tournament");
            logObjList.Add(new object[] { players, fromTournament });

            log = JsonConvert.SerializeObject(logObjList, Formatting.Indented);
            Debug.Log(log);

            return fromTournament;
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
            await DataAccess.SendData(tournamentInfo.Id, JsonConvert.SerializeObject(tournamentInfo, Formatting.Indented), Keys.TournamentsKey);
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

        private class PlayerMetadata
        {
            public bool HadByeRound;
            public int[] Record;
            public List<string> FacedOpponents = new List<string>();
        }

        public static TournamentInfo AdvanceTournament (TournamentInfo from, PlayerInfo[] players)
        {
            string log = "";
            return AdvanceTournament(from, players, ref log);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tournament/Test Tournament")]
        public static void TestTournament ()
        {
            var stream = System.IO.File.ReadAllText(Application.dataPath + "/tournament-test.json", System.Text.Encoding.UTF8);
            object[] data = JsonConvert.DeserializeObject<object[]>(stream);
            PlayerInfo[] players = JsonConvert.DeserializeObject<PlayerInfo[]>(data[0].ToString());
            //object[] playersObjArray = (object[])data[0];
            //PlayerInfo[] players = new PlayerInfo[playersObjArray.Length];
            //for (int i = 0; i < playersObjArray.Length; i++)
            //    players[i] = (PlayerInfo)playersObjArray[i];
            TournamentInfo tournament = JsonConvert.DeserializeObject<TournamentInfo>(data[1].ToString());
            string log = "";
            AdvanceTournament(tournament, players, ref log);
            if (!string.IsNullOrEmpty(log))
                System.IO.File.WriteAllText(Application.dataPath + "/tournament-test-result.json", log, System.Text.Encoding.UTF8);

            /*
            PlayerInfo[] players = new PlayerInfo[]
            {
                new PlayerInfo ()
                {
                    Id = "A",
                    IsBot = false,
                    Nickname = "A",
                    IsMasterClient = true,
                    IsMe = true,
                    CustomData = new Dictionary<string, object>()
                    {
                        { Keys.IsByeKey, false },
                        { Keys.MatchRecordKey, "3-2" },
                        { Keys.TournamentRecordKey, "0-0" },
                        { Keys.ClientStateKey, "Waiting" },
                    }
                },
                new PlayerInfo ()
                {
                    Id = "B",
                    IsBot = true,
                    Nickname = "B",
                    IsMasterClient = false,
                    IsMe = false,
                    CustomData = new Dictionary<string, object>()
                    {
                        { Keys.IsByeKey, false },
                        { Keys.MatchRecordKey, "2-3" },
                        { Keys.TournamentRecordKey, "0-0" },
                        { Keys.ClientStateKey, "Waiting" },
                    }
                },
                new PlayerInfo ()
                {
                    Id = "C",
                    IsBot = true,
                    Nickname = "C",
                    IsMasterClient = false,
                    IsMe = false,
                    CustomData = new Dictionary<string, object>()
                    {
                        { Keys.IsByeKey, false },
                        { Keys.MatchRecordKey, "0-3" },
                        { Keys.TournamentRecordKey, "0-0" },
                        { Keys.ClientStateKey, "Waiting" },
                    }
                },
                new PlayerInfo ()
                {
                    Id = "D",
                    IsBot = true,
                    Nickname = "D",
                    IsMasterClient = false,
                    IsMe = false,
                    CustomData = new Dictionary<string, object>()
                    {
                        { Keys.IsByeKey, false },
                        { Keys.MatchRecordKey, "3-0" },
                        { Keys.TournamentRecordKey, "0-0" },
                        { Keys.ClientStateKey, "Waiting" },
                    }
                },
                new PlayerInfo ()
                {
                    Id = "E",
                    IsBot = true,
                    Nickname = "E",
                    IsMasterClient = false,
                    IsMe = false,
                    CustomData = new Dictionary<string, object>()
                    {
                        { Keys.IsByeKey, true },
                        { Keys.MatchRecordKey, "0-0" },
                        { Keys.TournamentRecordKey, "1-0" },
                        { Keys.ClientStateKey, "Waiting" },
                    }
                },
            };
            TournamentInfo testTournament = new TournamentInfo()
            {
                Id = "Tournament1",
                IsOver = false,
                Players = new string[] { "A", "B", "C", "D", "E" },
                Rounds = new RoundInfo[]
                {
                    new RoundInfo ()
                    {
                        Id = "Round1",
                        Index = 0,
                        IsOver = true,
                        Matches = new MatchInfo[]
                        {
                            new MatchInfo ()
                            {
                                Id = "1",
                                IsOver = true,
                                Player1 = "A",
                                Player2 = "B",
                                Player1Wins = 3,
                                Player2Wins = 2
                            },
                            new MatchInfo ()
                            {
                                Id = "2",
                                IsOver = true,
                                Player1 = "C",
                                Player2 = "D",
                                Player1Wins = 0,
                                Player2Wins = 3
                            },
                            new MatchInfo ()
                            {
                                Id = "3",
                                IsOver = true,
                                Player1 = "E",
                                Player2 = "",
                                Player1Wins = 0,
                                Player2Wins = 0
                            },
                        }
                    }
                }
            };
            AdvanceTournament(testTournament, players);
            */
        }
#endif
    }
}
