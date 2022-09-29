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
                if (addZeroedRecords)
                {
                    dict.Add(Keys.IsByeKey, isBye);
                    dict.Add(Keys.MatchRecordKey, "0-0");
                    dict.Add(Keys.TournamentRecordKey, isBye ? "1-0" : "0-0");
                    players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(dict);
                }
                if (!isBye)
                {
                    if (addZeroedRecords)
                    {
                        dict.Clear();
                        dict.Add(Keys.IsByeKey, false);
                        dict.Add(Keys.MatchRecordKey, "0-0");
                        dict.Add(Keys.TournamentRecordKey, "0-0");
                        players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(dict);
                    }
                }
                int matchIndex = (int)Mathf.Ceil(i / 2f);
                matches[matchIndex] = new MatchInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    Player1 = players[i].Id,
                    Player2 = isBye ? null : players[i + 1].Id,
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

        public static TournamentInfo AdvanceTournament (TournamentInfo fromTournament, PlayerInfo[] players)
        {
            List<object> log = new List<object>();

            log.Add("Tournament Before");
            log.Add(new TournamentInfo(fromTournament));

            PlayerInfo[] Clone (PlayerInfo[] players)
            {
                PlayerInfo[] playersClone = new PlayerInfo[players.Length];
                for (int i = 0; i < players.Length; i++)
                    if (players[i] != null)
                        playersClone[i] = new PlayerInfo(players[i]);
                return playersClone;
            }
            log.Add("Players Before");
            log.Add(Clone(players));

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
                if (p1 != null )
                    UpdatePlayerRecord(p1, item.Player1Wins > item.Player2Wins); // TODO Consider bye
                if (p2 != null)
                    UpdatePlayerRecord(p2, item.Player2Wins > item.Player1Wins);
                if (item.Player1Wins == item.Player2Wins && item.Player1Wins > 0)
                    Debug.LogWarning($"Advance Tournament is not prepared to received Draws! Match between {p1?.Nickname} and {p2?.Nickname}");
            }
            log.Add("Updated Records");
            log.Add(Clone(players));

            // Just two players and they have already faced each other (as this functions should only be called after a match)
            if (players.Length == 2)
            {
                fromTournament.IsOver = true;
                return fromTournament;
            }

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
                            playerMetadata.FacedOpponents.Add(match.Player2);
                        else if (match.Player2 == player.Id)
                            playerMetadata.FacedOpponents.Add(match.Player1);
                    }
                }
                playerMetadata.Record = ExtractRecord(player);
                metadata.Add(player.Id, playerMetadata);
            }
            log.Add("Metadata");
            log.Add(metadata.ToObjArray());

            // Create new array with an empty spot if the number of players is odd
            PlayerInfo[] playersEven = new PlayerInfo[players.Length + players.Length % 2];
            for (int i = 0; i < players.Length; i++)
                playersEven[i] = players[i];

            // Shuffle and sort
            playersEven.Shuffle();
            Array.Sort(playersEven, SortBasedOnRecord);

            log.Add("Players Even Before");
            log.Add(Clone(playersEven));

            // Order players for new matches
            // TODO Get bye first if needed, then go for matches. Make more passes to guarantee no rematches
            for (int i = 0; i < playersEven.Length; i += 2)
            {
                if (playersEven[i] == null)
                {
                    playersEven[i] = playersEven[i + 1];
                    playersEven[i + 1] = null;
                }
                PlayerInfo p1 = playersEven[i];
                PlayerInfo p2 = playersEven[i + 1];
                if (metadata[p1.Id].FacedOpponents.Contains(p2 == null ? "" : p2.Id))
                {
                    for (int j = i + 2; j < playersEven.Length; j++)
                    {
                        if (!metadata[p1.Id].FacedOpponents.Contains(playersEven[j] == null ? "" : playersEven[j].Id))
                        {
                            p2 = playersEven[j];
                            playersEven[j] = playersEven[i + 1];
                            playersEven[i + 1] = p2;
                        }
                    }
                }
            }

            log.Add("Players Even After");
            log.Add(playersEven);

            // Return to players
            int playersIndex = 0;
            for (int i = 0; i < playersEven.Length; i++)
            {
                if (playersEven[i] == null)
                    continue;
                players[playersIndex] = playersEven[i];
                playersIndex++;
            }

            log.Add("Players Returned");
            log.Add(Clone(players));

            // Add new round
            fromTournament.Rounds = fromTournament.Rounds.CloneWithAdd(CreateRound(players, false));

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

            log.Add("Tournament After");
            log.Add(fromTournament);
            log.Add("Players At The End"); // TODO Zero out match records
            log.Add(players);

            Debug.Log(JsonConvert.SerializeObject(log));

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
            public string Id;
            public int[] Record;
            public List<string> FacedOpponents = new List<string>();
        }
    }
}
