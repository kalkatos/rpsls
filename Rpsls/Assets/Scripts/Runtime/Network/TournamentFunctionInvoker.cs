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

        private static TournamentInfo CreateTournament (PlayerInfo[] players)
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
                players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(Keys.IsByeKey, isBye);
                players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, "0-0");
                players[i].CustomData = players[i].CustomData.CloneWithUpdateOrAdd(Keys.TournamentRecordKey, isBye ? "1-0" : "0-0");
                if (!isBye)
                {
                    playerIds[i + 1] = players[i + 1].Id;
                    players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(Keys.IsByeKey, false);
                    players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(Keys.MatchRecordKey, "0-0");
                    players[i + 1].CustomData = players[i + 1].CustomData.CloneWithUpdateOrAdd(Keys.TournamentRecordKey, "0-0");
                }
                int matchIndex = (int)Mathf.Ceil(i / 2f);
                matches[matchIndex] = new MatchInfo()
                {
                    Id = matchIndex,
                    Player1 = players[i],
                    Player2 = isBye ? null : players[i + 1],
                    Player1Wins = 0,
                    Player2Wins = 0
                };
            }
            tournament.Players = playerIds;
            tournament.Rounds = new RoundInfo[]
            {
                new RoundInfo()
                {
                    index = 0,
                    Matches = matches
                }
            };
            return tournament;
        }

        private static object AdvanceTournament (object fromTournament)
        {
            return null;
        }

        public static async Task<object> StartTournament (object playersObj)
        {
            // Check parameters
            if (!(playersObj is object[]))
                return Error.WrongParameters;
            object[] playersArrayObj = (object[])playersObj;
            PlayerInfo[] players = new PlayerInfo[playersArrayObj.Length];
            for (int i = 0; i < playersArrayObj.Length; i++)
            {
                if (!(playersArrayObj[i] is PlayerInfo))
                    return Error.WrongParameters;
                players[i] = (PlayerInfo)playersArrayObj[i]; 
            }
            // Check if it's master
            if (!IsMasterClient)
                return Error.MustBeMaster;
            // Create tournament
            TournamentInfo tournamentInfo = CreateTournament(players);
            // Update players
            foreach (var item in players)
                await DataAccess.SendData(item.Id, JsonConvert.SerializeObject(item), Keys.ConnectedPlayersKey);
            return tournamentInfo;
        }

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
            string tmtListSerialized = (await GetData(Keys.TournamentListKey)).ToString();
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
