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

        public static TournamentInfo CreateTournament (PlayerInfo[] players)
        {
            players.Shuffle();
            TournamentInfo tournament = new TournamentInfo();
            tournament.Id = Guid.NewGuid().ToString();
            string[] playerIds = new string[players.Length];
            MatchInfo[] matches = new MatchInfo[(int)Mathf.Ceil(players.Length / 2f)];
            for (int i = 0; i < players.Length; i += 2)
            {
                bool isBye = i == players.Length - 1;
                playerIds[i] = players[i].Id;
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(Keys.IsByeKey, isBye);
                dict.Add(Keys.MatchRecordKey, "0-0");
                dict.Add(Keys.TournamentRecordKey, isBye ? "1-0" : "0-0");
                players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(dict);
                if (!isBye)
                {
                    playerIds[i + 1] = players[i + 1].Id;
                    dict.Clear();
                    dict.Add(Keys.IsByeKey, false);
                    dict.Add(Keys.MatchRecordKey, "0-0");
                    dict.Add(Keys.TournamentRecordKey, "0-0");
                    players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(dict);
                }
                int matchIndex = (int)Mathf.Ceil(i / 2f);
                matches[matchIndex] = new MatchInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    Player1 = players[i].Id,
                    Player2 = isBye ? null : players[i + 1].Id,
                    Player1Wins = 0,
                    Player2Wins = 0
                };
            }
            tournament.Players = playerIds;
            tournament.Rounds = new RoundInfo[]
            {
                new RoundInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    Index = 0,
                    Matches = matches
                }
            };
            return tournament;
        }

        public static TournamentInfo AdvanceTournament (TournamentInfo fromTournament, PlayerInfo[] players)
        {
            foreach (var item in players)
            {
                if (!item.CustomData.ContainsKey(Keys.TournamentRecordKey))
                    item.CustomData.Add(Keys.TournamentRecordKey, "0-0");
            }
            Array.Sort(players, SortBasedOnRecord);

            int SortBasedOnRecord (PlayerInfo p1, PlayerInfo p2)
            {
                // TODO Sort
                return 0;
            }
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
    }
}
