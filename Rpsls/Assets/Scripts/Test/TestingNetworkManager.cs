using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using Kalkatos.Network;
using Newtonsoft.Json;

namespace Kalkatos.Rpsls.Test
{
    public class TestingNetworkManager : NetworkManager
    {
        private const string connectedPlayersKey = "CntPl";
        private const string activeRoomsKey = "AtvRm";
        private char[] functionCallSeparator = new char[] { '[', ']' };
        private char[] functionDataSeparator = new char[] { ';' };
        private char[] functionArgumentsSeparator = new char[] { '(', ',', ')' };

        private string playerId;
        private string roomId;
        private EventRemovalOption eventRemovalOption = EventRemovalOption.AfterAllReceived;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        private Dictionary<string, PlayerInfo> lastKnownPlayers = new Dictionary<string, PlayerInfo>();
        private Dictionary<string, PlayerInfo> connectedPlayers = new Dictionary<string, PlayerInfo>();
        private Dictionary<string, RoomInfo> activeRooms = new Dictionary<string, RoomInfo>();
        private Dictionary<string, EventExecution> eventExecutions = new Dictionary<string, EventExecution>();

        private string executeEventKey => "ExEvt" + roomId;
        private string newSmallGuid => Guid.NewGuid().ToString().Split('-')[0];
        private float randomTime => Random.Range(0.3f, 1f);

        public override bool IsInRoom { get => !string.IsNullOrEmpty(roomId); }
        public override PlayerInfo MyPlayerInfo { get => connectedPlayers[playerId]; protected set => connectedPlayers[playerId] = value; }
        public override RoomInfo CurrentRoomInfo { get => activeRooms[roomId]; protected set => activeRooms[roomId] = value; }

        private void OnApplicationQuit ()
        {
            LoadLists();
            if (IsConnected && IsInRoom)
                LeaveRoom();
            if (connectedPlayers.Count <= 1)
            {
                Debug.Log("[Testing] Cleaning up to quit.");
                SaveManager.DeleteKey(connectedPlayersKey);
                SaveManager.DeleteKey(activeRoomsKey);
            }
            else
            {
                connectedPlayers.Remove(playerId);
                SaveLists();
            }
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
                        connectedPlayers.Add(currentKey, JsonConvert.DeserializeObject<PlayerInfo>(objArray[i].ToString()));
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
                        for (int j = 0; j < room.Players.Count; j++)
                            room.Players[j] = connectedPlayers[room.Players[j].Id];
                        activeRooms.Add(currentKey, room);
                    }
                }
            }
        }

        private void SaveLists ()
        {
            SaveManager.SaveString(connectedPlayersKey, JsonConvert.SerializeObject(connectedPlayers.ToObjArray(), Formatting.None));
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
                            else if (player.CustomData != lastKnownPlayers[player.Id].CustomData ||
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
                            Debug.Log("Event received " + item.Value.EventKey);
                            RaiseEventReceived(item.Value.EventKey, item.Value.Parameters);
                            string[] newPlayers = new string[item.Value.PlayersWhoExecuted.Length + 1];
                            newPlayers[0] = playerId;
                            for (int i = 0; i < item.Value.PlayersWhoExecuted.Length; i++)
                                newPlayers[i + 1] = item.Value.PlayersWhoExecuted[i];

                            if (eventRemovalOption == EventRemovalOption.AfterAllReceived)
                            {
                                int countOfPlayersFound = 0;
                                List<PlayerInfo> playersInRoom = activeRooms[roomId].Players;
                                for (int i = 0; i < playersInRoom.Count; i++)
                                    if (newPlayers.Contains(playersInRoom[i].Id))
                                        countOfPlayersFound++;
                                if (countOfPlayersFound == newPlayers.Length)
                                    executionsToRemove.Add(item.Key);
                            }

                            item.Value.PlayersWhoExecuted = newPlayers;
                        }
                    }
                    if (eventRemovalOption == EventRemovalOption.AfterAllReceived)
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

        public override void Connect ()
        {
            this.Wait(randomTime, () =>
            {
                IsConnected = true;
                Debug.Log("[Testing] Connected");
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
            LoadLists();
            connectedPlayers[playerId].Nickname = name;
            SaveLists();
            SaveManager.SaveNickname(name);
        }

        public override void LogIn (object parameter = null)
        {
            Assert.IsTrue(IsConnected);

            this.Wait(randomTime, (Action)(() =>
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
                Debug.Log("[Testing] Logged In");
                base.RaiseLogInSuccess();
            }));
        }

        public override void FindMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsNotNull(parameter);

            if (parameter is RoomOptions)
            {
                RoomOptions options = (RoomOptions)parameter;
                Debug.Log("[Testing] Creating Room");
                this.Wait(randomTime, (Action)(() =>
                {
                    LoadLists();
                    connectedPlayers[playerId].IsMasterClient = true;
                    RoomInfo newRoom = new RoomInfo()
                    {
                        Id = CreateRandomRoomName(),
                        MaxPlayers = options.MaxPlayers,
                        Players = new List<PlayerInfo>() { MyPlayerInfo },
                        PlayerCount = 1
                    };
                    roomId = newRoom.Id;
                    activeRooms.Add(newRoom.Id, newRoom);
                    SaveLists();
                    base.RaiseFindMatchSuccess();
                    base.RaisePlayerEnteredRoom(MyPlayerInfo);
                    Debug.Log($"[Testing] Room created: {newRoom.Id}");
                }));
            }
            else if (parameter is string)
            {
                this.Wait(randomTime, (Action)(() =>
                {
                    string wantedRoom = (string)parameter;
                    LoadLists();
                    if (activeRooms.ContainsKey(wantedRoom))
                    {
                        RoomInfo room = activeRooms[wantedRoom];
                        List<PlayerInfo> playersInRoom = room.Players;
                        connectedPlayers[playerId].IsMasterClient = false;
                        playersInRoom.Add(MyPlayerInfo);
                        room.PlayerCount = playersInRoom.Count;
                        roomId = room.Id;
                        SaveLists();
                        base.RaiseFindMatchSuccess();
                        base.RaisePlayerEnteredRoom(MyPlayerInfo);
                        Debug.Log($"[Testing] Entering room: {room.Id}");
                    }
                    else
                    {
                        base.RaiseFindMatchFailure();
                        Debug.Log($"[Testing] Room not found: {wantedRoom}");
                    }
                }));
            }
        }

        public override void LeaveMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            LoadLists();
            LeaveRoom();
            SaveLists();
            Debug.Log("[Testing] Left Room.");
        }

        public override void SetMyCustomData (object parameter = null)
        {
            Assert.IsTrue(IsConnected);

            LoadLists();
            connectedPlayers[playerId].CustomData = parameter;
            SaveLists();
            RaisePlayerDataChanged(MyPlayerInfo, parameter);
        }

        public override void SendData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            this.Wait(randomTime, () =>
            {
                string key = "";
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i % 2 == 0)
                        key = (string)parameters[i];
                    else if (!data.ContainsKey(key))
                        data.Add(key, parameters[i]);
                    else
                        data[key] = parameters[i];
                }
                RaiseSendDataSuccess();
            });
        }

        public override void RequestData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            this.Wait(randomTime, () =>
            {
                string key = (string)parameters[0];
                if (data.ContainsKey(key))
                    RaiseRequestDataSuccess(key, data[key]);
                else
                    RaiseRequestDataFailure(key);
            });
        }

        public override void ExecuteEvent (string eventKey, params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            Debug.Log("Sent event: " + eventKey);
            EventExecution execution = new EventExecution()
            {
                Id = newSmallGuid,
                EventKey = eventKey,
                Parameters = parameters,
                PlayersWhoExecuted = new string[] { playerId }
            };
            RaiseEventReceived(eventKey, parameters);
            LoadEventExecutions();
            eventExecutions.Add(execution.Id, execution);
            SaveEventExecutions();
        }
    }

    internal class EventExecution
    {
        public string Id;
        public string EventKey;
        public object[] Parameters;
        public string[] PlayersWhoExecuted;
    }

    internal enum EventRemovalOption
    {
        AfterAllReceived,
        Delayed
    }
}
