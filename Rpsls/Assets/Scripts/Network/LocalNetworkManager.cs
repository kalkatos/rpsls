using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

namespace Kalkatos.Network
{
    [DefaultExecutionOrder(-10)]
    public class LocalNetworkManager : NetworkManager
    {
        [Header("Debug")]
        [SerializeField] private bool bypassConnection;
        [SerializeField] private int amountOfPlayersToWaitFor;
        [SerializeField] private int amountOfPlayersToFillIn;

        private const string connectedPlayersKey = "CntPl";
        private const string activeRoomsKey = "AtvRm";
        private const string customDataKey = "CtmDt";
        private const string roomChangedKey = "RmChd";
        private const string roomOpenKey = "RmOpn";
        private const string roomCloseKey = "RmCls";

        private string playerId;
        private string roomId;
        private float eventLifetime = 300;
        private Dictionary<string, PlayerInfo> lastKnownPlayers = new Dictionary<string, PlayerInfo>();
        private Dictionary<string, PlayerInfo> connectedPlayers = new Dictionary<string, PlayerInfo>();
        private Dictionary<string, RoomInfo> activeRooms = new Dictionary<string, RoomInfo>();
        private Dictionary<string, EventExecution> eventExecutions = new Dictionary<string, EventExecution>();

        private string executeEventKey => "ExEvt" + roomId;
        private string newSmallGuid => Guid.NewGuid().ToString().Split('-')[0];
        private float randomTime => bypassConnection ? 0 : Random.Range(0.3f, 1f);

        public override bool IsInRoom { get => !string.IsNullOrEmpty(roomId); }
        public override PlayerInfo MyPlayerInfo { get => connectedPlayers[playerId]; protected set => connectedPlayers[playerId] = value; }
        public override RoomInfo CurrentRoomInfo
        {
            get
            {
                RoomInfo room = activeRooms[roomId];
                for (int i = 0; i < room.Players.Count; i++)
                {
                    PlayerInfo player = room.Players[i];
                    player.IsMe = player.Id == playerId;
                    room.Players[i] = connectedPlayers[player.Id]; 
                }
                return room;
            }
            protected set
            {
                activeRooms[roomId] = value;
            }
        }

        private void OnApplicationQuit ()
        {
            if (bypassConnection && MyPlayerInfo.IsMasterClient)
            {
                SaveManager.DeleteKey(connectedPlayersKey);
                SaveManager.DeleteKey(activeRoomsKey);
                SaveManager.DeleteKey(executeEventKey);
                SaveManager.DeleteKey(customDataKey);
                return;
            }

            LoadLists();
            string savedExecEventKey = executeEventKey;
            if (IsConnected && IsInRoom)
                LeaveRoom();
            if (connectedPlayers.Count <= 1)
            {
                this.Log("Cleaning up to quit.");
                connectedPlayers.Clear();
                SaveManager.DeleteKey(connectedPlayersKey);
                SaveManager.DeleteKey(activeRoomsKey);
                SaveManager.DeleteKey(savedExecEventKey);
            }
            else
                connectedPlayers.Remove(playerId);
            SaveLists();
        }
        
        protected override void OnAwake ()
        {
            if (bypassConnection)
                Connect();
            LocalFunctionServer.ExecuteFunction("Foo", new object[] { "Crazy Babies" }, 
                (successPar) => Debug.Log("Received success parameter: " + successPar), 
                (failurePar) => Debug.Log("Received failure parameter: " + failurePar));
        }

        private void LoadLists ()
        {
            if (SaveManager.HasKey(connectedPlayersKey))
            {
                object[] objArray = JsonConvert.DeserializeObject<object[]>(SaveManager.GetString(connectedPlayersKey, ""));
                connectedPlayers = new Dictionary<string, PlayerInfo>();
                string currentKey = "";
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (i % 2 == 0)
                        currentKey = objArray[i].ToString();
                    else
                    {
                        PlayerInfo info = JsonConvert.DeserializeObject<PlayerInfo>(objArray[i].ToString());
                        info.IsMe = info.Id == playerId;
                        connectedPlayers.Add(currentKey, info);
                    }
                }
            }
            if (SaveManager.HasKey(activeRoomsKey))
            {
                object[] objArray = JsonConvert.DeserializeObject<object[]>(SaveManager.GetString(activeRoomsKey, ""));
                activeRooms = new Dictionary<string, RoomInfo>();
                string currentKey = "";
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (i % 2 == 0)
                        currentKey = objArray[i].ToString();
                    else
                    {
                        RoomInfo room = JsonConvert.DeserializeObject<RoomInfo>(objArray[i].ToString());
                        //if (room.CustomData != null)
                        //    room.CustomData = JsonConvert.DeserializeObject<object[]>(room.CustomData.ToString()).ToDictionary();
                        for (int j = 0; j < room.Players.Count; j++)
                            room.Players[j] = connectedPlayers[room.Players[j].Id];
                        activeRooms.Add(currentKey, room);
                    }
                }
            }
        }

        private void SaveLists ()
        {
            if (connectedPlayers.ContainsKey(playerId))
                connectedPlayers[playerId].IsMe = false;
            SaveManager.SaveString(connectedPlayersKey, JsonConvert.SerializeObject(connectedPlayers.ToObjArray(), Formatting.None));
            if (connectedPlayers.ContainsKey(playerId))
                connectedPlayers[playerId].IsMe = true;
            SaveManager.SaveString(activeRoomsKey, JsonConvert.SerializeObject(activeRooms.ToObjArray(), Formatting.None));
        }

        private void LoadEventExecutions ()
        {
            if (SaveManager.HasKey(executeEventKey))
            {
                object[] objArray = JsonConvert.DeserializeObject<object[]>(SaveManager.GetString(executeEventKey, ""));
                eventExecutions = new Dictionary<string, EventExecution>();
                string currentKey = "";
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (i % 2 == 0)
                        currentKey = objArray[i].ToString();
                    else
                        eventExecutions.Add(currentKey, JsonConvert.DeserializeObject<EventExecution>(objArray[i].ToString()));
                }
            }
        }

        private void SaveEventExecutions ()
        {
            SaveManager.SaveString(executeEventKey, JsonConvert.SerializeObject(eventExecutions.ToObjArray(), Formatting.None));
        }

        private IEnumerator CheckCoroutine ()
        {
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return wait;
                if (IsInRoom)
                {
                    LoadLists();
                    //Check players
                    List<PlayerInfo> roomPlayerList = CurrentRoomInfo.Players;
                    foreach (var player in roomPlayerList)
                    {
                        bool isKnownPlayer = lastKnownPlayers.ContainsKey(player.Id);
                        if (player.Id != playerId)
                        {
                            if (!isKnownPlayer)
                                RaisePlayerEnteredRoom(player);
                            else if (!player.CustomData.IsEqual(lastKnownPlayers[player.Id].CustomData) ||
                                player.Nickname != lastKnownPlayers[player.Id].Nickname)
                                RaisePlayerDataChanged(player);
                        }
                        if (isKnownPlayer && player.IsMasterClient && !lastKnownPlayers[player.Id].IsMasterClient)
                            RaiseMasterClientChanged(player);
                    }
                    foreach (var item in lastKnownPlayers)
                        if (item.Key != playerId && roomPlayerList.Find((player) => player.Id == item.Key) == null)
                            RaisePlayerLeftRoom(item.Value);
                    lastKnownPlayers.Clear();
                    for (int i = 0; i < roomPlayerList.Count; i++)
                        lastKnownPlayers.Add(roomPlayerList[i].Id, roomPlayerList[i]);

                    //Check events
                    LoadEventExecutions();
                    List<string> executionsToRemove = new List<string>();
                    foreach (var item in eventExecutions)
                    {
                        if (!item.Value.PlayersWhoExecuted.Contains(playerId))
                        {
                            this.Log("Event received " + item.Value.EventKey);
                            RaiseEventReceived(item.Value.EventKey, item.Value.Parameters);
                            item.Value.PlayersWhoExecuted.Add(playerId);
                        }
                        if (DateTime.Now > item.Value.RemoveAt)
                            executionsToRemove.Add(item.Value.Id);
                    }
                    for (int i = 0; i < executionsToRemove.Count; i++)
                        eventExecutions.Remove(executionsToRemove[i]);
                    SaveEventExecutions();
                }
            }
        }

        private void LeaveRoom ()
        {
            RoomInfo room = activeRooms[roomId];
            List<PlayerInfo> players = room.Players;
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i].Id == playerId)
                {
                    PlayerInfo myPlayerInfo = players[i];
                    players.RemoveAt(i);
                    if (players.Count > 0 && myPlayerInfo.IsMasterClient)
                        connectedPlayers[players[0].Id].IsMasterClient = true;
                    myPlayerInfo.IsMasterClient = false;
                    RaisePlayerLeftRoom(myPlayerInfo);
                    break;
                }
            }
            roomId = "";
            room.PlayerCount = players.Count;
            if (players.Count == 0)
            {
                activeRooms.Remove(room.Id);
                SaveManager.DeleteKey(executeEventKey);
                if (activeRooms.Count == 0)
                    SaveManager.DeleteKey(activeRoomsKey);
            }
            else
                activeRooms[room.Id] = room;
        }

        #region ==================  Requests  ===========================

        public override void Connect ()
        {
            this.Wait(randomTime, () =>
            {
                IsConnected = true;
                this.Log("Connected");
                LogIn();
            });
        }

        public override string GetPlayerName ()
        {
            string savedName = SaveManager.GetNickname();
            if (string.IsNullOrEmpty(savedName))
            {
                savedName = CreateGuestName();
                SetPlayerName(savedName);
            }
            return savedName;
        }

        public override void SetPlayerName (string name)
        {
            Assert.IsTrue(IsConnected);

            LoadLists();
            connectedPlayers[playerId].Nickname = name;
            SaveLists();
            SaveManager.SaveNickname(name);
        }

        public override void LogIn (object parameter = null)
        {
            Assert.IsTrue(IsConnected);

            this.Wait(randomTime, () =>
            {
                LoadLists();
                bool isFirst = connectedPlayers == null || connectedPlayers.Count == 0;
                playerId = newSmallGuid;
                PlayerInfo myPlayerInfo = new PlayerInfo()
                {
                    Id = playerId,
                    Nickname = SaveManager.GetNickname(),
                    IsMasterClient = false,
                };
                connectedPlayers.Add(playerId, myPlayerInfo);
                SaveLists();
                StartCoroutine(CheckCoroutine());
                this.Log("Logged In");
                RaiseLogInSuccess();
            });
        }

        public override void FindMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);

            if (bypassConnection)
            {
                LoadLists();
                if (activeRooms.TryGetValue("DEBUG", out RoomInfo room))
                    parameter = "DEBUG";
                else
                    parameter = new RoomOptions() { DesiredName = "DEBUG", MaxPlayers = 8 };
            }
            else
                Assert.IsNotNull(parameter);

            if (parameter is RoomOptions)
            {
                RoomOptions options = (RoomOptions)parameter;
                this.Log("Creating Room");
                this.Wait(randomTime, () =>
                {
                    LoadLists();
                    connectedPlayers[playerId].IsMasterClient = true;
                    RoomInfo newRoom = new RoomInfo()
                    {
                        Id = string.IsNullOrEmpty(options.DesiredName) ? CreateRandomRoomName() : options.DesiredName,
                        MaxPlayers = options.MaxPlayers,
                        Players = new List<PlayerInfo>() { connectedPlayers[playerId] },
                        PlayerCount = 1
                    };
                    roomId = newRoom.Id;
                    activeRooms.Add(newRoom.Id, newRoom);
                    SaveLists();
                    RaiseFindMatchSuccess();
                    RaisePlayerEnteredRoom(MyPlayerInfo);
                    this.Log($"Room created: {newRoom.Id}");
                });
            }
            else if (parameter is string)
            {
                this.Wait(randomTime, () =>
                {
                    string wantedRoom = (string)parameter;
                    LoadLists();
                    if (activeRooms.ContainsKey(wantedRoom))
                    {
                        RoomInfo room = activeRooms[wantedRoom];
                        if (room.IsClosed)
                        {
                            RaiseFindMatchFailure(FindMatchError.RoomIsClosed);
                            this.Log($"Room is closed: {wantedRoom}");
                        }
                        else if (room.PlayerCount >= room.MaxPlayers)
                        {
                            RaiseFindMatchFailure(FindMatchError.RoomIsFull);
                            this.Log($"Room is full: {wantedRoom}");
                        }
                        else
                        {
                            List<PlayerInfo> playersInRoom = room.Players;
                            connectedPlayers[playerId].IsMasterClient = false;
                            playersInRoom.Add(MyPlayerInfo);
                            room.PlayerCount = playersInRoom.Count;
                            roomId = room.Id;
                            SaveLists();
                            RaiseFindMatchSuccess();
                            RaisePlayerEnteredRoom(MyPlayerInfo);
                            this.Log($"Entering room: {room.Id}");
                        }
                    }
                    else
                    {
                        RaiseFindMatchFailure(FindMatchError.RoomNotFound);
                        this.Log($"Room not found: {wantedRoom}");
                    }
                });
            }
            else
            {
                RaiseFindMatchFailure(FindMatchError.WrongParameters);
                this.LogError("Find Match expects a RoomOptions or string object as parameter but it is something else."); 
            }
        }

        public override void LeaveMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            LoadLists();
            LeaveRoom();
            SaveLists();
            this.Log("Left Room.");
        }

        public override void UpdateMyCustomData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);

            LoadLists();
            Dictionary<string, object> myData = connectedPlayers[playerId].CustomData;
            connectedPlayers[playerId].CustomData = myData.UpdateOrAdd(parameters.ToDictionary());
            SaveLists();
            RaisePlayerDataChanged(MyPlayerInfo);
        }

        public override void UpdateRoomData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            if (CurrentRoomInfo.Players.Find((player) => player.Id == playerId) != null)
            {
                LoadLists();
                Dictionary<string, object> roomData = activeRooms[roomId].CustomData;
                roomData = roomData.UpdateOrAdd(parameters.ToDictionary());
                BroadcastEvent(roomChangedKey, roomData.ToObjArray());
            }
        }

        public override void SendData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            this.Wait(randomTime, () =>
            {
                object[] loadedData = JsonConvert.DeserializeObject<object[]>(SaveManager.GetString(customDataKey, ""));
                loadedData = loadedData.CloneWithChange(parameters);
                SaveManager.SaveString(customDataKey, JsonConvert.SerializeObject(loadedData));
                RaiseSendDataSuccess();
            });
        }

        public override void RequestData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            this.Wait(randomTime, () =>
            {
                Dictionary<string, object> loadedData = JsonConvert.DeserializeObject<object[]>(SaveManager.GetString(customDataKey, "")).ToDictionary();
                Dictionary<string, object> resultData = new Dictionary<string, object>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    string key = (string)parameters[i];
                    if (loadedData.ContainsKey(key))
                        resultData.Add(key, loadedData[key]);
                }
                if (resultData.Count > 0)
                    RaiseRequestDataSuccess(resultData.ToObjArray());
                else
                    RaiseRequestDataFailure(parameters);
            });
        }

        public override void BroadcastEvent (string eventKey, params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            this.Log("Sent event: " + eventKey);
            EventExecution execution = new EventExecution()
            {
                Id = newSmallGuid,
                EventKey = eventKey,
                RemoveAt = DateTime.Now.AddSeconds(eventLifetime),
                Parameters = parameters,
                PlayersWhoExecuted = new List<string>() { playerId }
            };
            RaiseEventReceived(eventKey, parameters);
            LoadEventExecutions();
            eventExecutions.Add(execution.Id, execution);
            SaveEventExecutions();
        }

        public override void OpenRoom (params object[] parameters)
        {
            Assert.IsTrue(IsInRoom);

            LoadLists();
            CurrentRoomInfo.IsClosed = false;
            SaveLists();
            BroadcastEvent(roomOpenKey, parameters);
        }

        public override void CloseRoom (params object[] parameters)
        {
            Assert.IsTrue(IsInRoom);

            LoadLists();
            CurrentRoomInfo.IsClosed = true;
            SaveLists();
            BroadcastEvent(roomCloseKey, parameters);
        }

        public override void ExecuteFunction (string functionName, params object[] parameters)
        {
            Assert.IsTrue(IsConnected);

            LocalFunctionServer.ExecuteFunction(functionName, parameters, 
                (success) =>
                {
                    RaiseExecuteFunctionSuccess((object[])success);
                },
                (failure) =>
                {
                    RaiseExecuteFunctionFailure("Message", failure.ToString());
                });
        }

        #endregion

        public override void RaiseLogInSuccess (object parameter = null)
        {
            base.RaiseLogInSuccess(parameter);
            if (bypassConnection)
            {
                FindMatch();
                if (amountOfPlayersToWaitFor > 1)
                {
                    while (true)
                    {
                        LoadLists();
                        if (connectedPlayers.Count >= amountOfPlayersToWaitFor)
                            break;
                        float delayStart = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - delayStart < 0.2f) { }
                    }
                }
                if (MyPlayerInfo.IsMasterClient && amountOfPlayersToFillIn > 0)
                {
                    LoadLists();
                    for (int i = 0; i < amountOfPlayersToFillIn; i++)
                    {
                        string newPlayerId = newSmallGuid;
                        PlayerInfo newPlayer = new PlayerInfo()
                        {
                            Id = newPlayerId,
                            Nickname = CreateGuestName(),
                            IsBot = true
                        };
                        connectedPlayers.Add(newPlayerId, newPlayer);
                        CurrentRoomInfo.Players.Add(newPlayer);
                    }
                    SaveLists();
                }
            }
        }

        public override void RaiseEventReceived (string eventKey, params object[] parameters)
        {
            if (IsInRoom)
            {
                if (eventKey == roomChangedKey)
                    CurrentRoomInfo.CustomData = parameters.ToDictionary();
                else if (eventKey == roomOpenKey)
                    RaiseRoomOpened(parameters);
                else if (eventKey == roomCloseKey)
                    RaiseRoomClosed(parameters);
            }
            base.RaiseEventReceived(eventKey, parameters);
        }
    }

    internal class EventExecution
    {
        public string Id;
        public string EventKey;
        public DateTime RemoveAt;
        public object[] Parameters;
        public List<string> PlayersWhoExecuted = new List<string>();
    }
}
