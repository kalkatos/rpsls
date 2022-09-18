using Kalkatos.Tournament;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Kalkatos.Network
{
    internal partial class LocalFunctionServer : MonoBehaviour
    {
        private static LocalFunctionServer instance;
        public static LocalFunctionServer Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject(nameof(LocalFunctionServer)).AddComponent<LocalFunctionServer>();
                return instance;
            }
        }

        private List<Tuple<Task<object>, Action<object>, Action<object>>> delayedExecutions =
            new List<Tuple<Task<object>, Action<object>, Action<object>>>();

        private static bool IsMasterClient => NetworkManager.Instance.MyPlayerInfo.IsMasterClient;

        private void Update ()
        {
            for (int i = delayedExecutions.Count - 1; i >= 0; i--)
            {
                var item = delayedExecutions[i];
                if (item.Item1.Status == TaskStatus.RanToCompletion)
                {
                    object result = item.Item1.Result;
                    if (result is Error)
                        item.Item3.Invoke(result);
                    else
                        item.Item2.Invoke(result);
                    delayedExecutions.RemoveAt(i);
                }
            }
        }

        internal static void ExecuteFunction (string name, object[] parameters, Action<object> success, Action<object> failure)
        {
            var methods = typeof(LocalFunctionServer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                string methodName = methods[i].Name;
                if (methodName == name && name != nameof(ExecuteFunction))
                {
                    object result = null;
                    try
                    {
                        object[] args = new object[] { parameters };
                        result = typeof(LocalFunctionServer).InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, args);
                    }
                    catch (Exception ex)
                    {
                        failure?.Invoke(ex.Message);
                        return;
                    }
                    if (result is Error)
                        failure?.Invoke(result);
                    else if (result is Task<object>)
                    {
                        Instance.delayedExecutions.Add(new Tuple<Task<object>, Action<object>, Action<object>>((Task<object>)result, success, failure));
                    }
                    else
                        success?.Invoke(result);
                    return;
                }
            }
            failure?.Invoke(Error.MethodNotFound);
        }

        private static object CreateTournament (object playersArrayObj)
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

        private static object AdvanceTournament (object fromTournament)
        {
            return null;
        }

        private static async Task<object> GetRoundInfo (object parameter)
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

        #region =================== Generic =================================

        private static async Task<object> GetData (string key)
        {
            string data = await Instance.GetString(key, "");
            if (string.IsNullOrEmpty(data))
                return null;
            return data;
        }

        internal virtual async Task<string> GetString (string key, string defaultValue)
        {
            //LOCAL
            string result = SaveManager.GetString(key, defaultValue);
            await Task.Delay(1);
            return result;
        }

        private static async Task SetString (string key, string value)
        {
            //LOCAL
            SaveManager.SaveString(key, value);
            await Task.Delay(1);
        }

        #endregion

        internal enum Error
        {
            WrongParameters,
            MethodNotFound,
            NotAvailable,
            NotAllowed,
            MustBeMaster,
        };
    }
}
