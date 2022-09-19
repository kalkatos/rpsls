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

        public static object CreateTournament (object playersArrayObj)
        {
            object[] playersObj = (object[])playersArrayObj;
            PlayerInfo[] players = new PlayerInfo[playersObj.Length];
            for (int i = 0; i < playersObj.Length; i++)
                players[i] = (PlayerInfo)playersObj[i];
            players.Shuffle();
            TournamentInfo tournament = new TournamentInfo();
            tournament.Id = Guid.NewGuid().ToString();
            string[] playerIds = new string[players.Length];
            MatchInfo[] matches = new MatchInfo[(int)Mathf.Ceil(players.Length / 2f)];
            for (int i = 0; i < players.Length; i += 2)
            {
                bool isBye = i == players.Length - 1;
                playerIds[i] = players[i].Id;
                players[i].CustomData = players[i].CustomData.UpdateOrAdd(Keys.IsByeKey, isBye);
                players[i].CustomData = players[i].CustomData.UpdateOrAdd(Keys.MatchRecordKey, "0-0");
                players[i].CustomData = players[i].CustomData.UpdateOrAdd(Keys.TournamentRecordKey, isBye ? "1-0" : "0-0");
                if (!isBye)
                {
                    playerIds[i + 1] = players[i + 1].Id;
                    players[i + 1].CustomData = players[i + 1].CustomData.UpdateOrAdd(Keys.IsByeKey, isBye);
                    players[i + 1].CustomData = players[i + 1].CustomData.UpdateOrAdd(Keys.MatchRecordKey, "0-0");
                    players[i + 1].CustomData = players[i + 1].CustomData.UpdateOrAdd(Keys.TournamentRecordKey, "0-0");
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

        public static object AdvanceTournament (object fromTournament)
        {
            return null;
        }

        public static async Task<object> GetRoundInfo (object parameter)
        {
            // Check parameters
            object[] parameters = (object[])parameter;
            var paramDict = parameters.ToDictionary();
            if (!paramDict.TryGetValue(Keys.TournamentIdKey, out object tournamentIdObj) ||
                !paramDict.TryGetValue(Keys.RoundNumberKey, out object roundNumberObj))
                return Error.WrongParameters;
            string tournamentId = tournamentIdObj.ToString();
            int roundNumber = int.Parse(roundNumberObj.ToString());
            // Check if the tournament exists
            string tmtListSerialized = (await GetData(Keys.TournamentListKey)).ToString();
            if (string.IsNullOrEmpty(tmtListSerialized))
                return Error.NotAvailable;
            Dictionary<string, object> tmtListDict = JsonConvert.DeserializeObject<object[]>(tmtListSerialized).ToDictionary();
            if (tmtListDict == null)
                return Error.NotAvailable;
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
            // Return round if it exists
            if (roundNumber < tournamentInfo.Rounds.Length)
                return tournamentInfo.Rounds[roundNumber];
            return Error.WrongParameters;
        }
    }
}
