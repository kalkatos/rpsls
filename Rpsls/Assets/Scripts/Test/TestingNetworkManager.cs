using System;
using System.Collections;
using System.Collections.Generic;
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

        private string playerLoginId;
        private List<PlayerInfo> lastKnownPlayerList = new List<PlayerInfo>();
        private Dictionary<string, object> data = new Dictionary<string, object>();

        private string executeFunctionKey => "ExFct" + CurrentRoomInfo.Id;
        private string smallGuid => Guid.NewGuid().ToString().Split('-')[0];
        private float randomTime => Random.Range(0.3f, 1f);
        private List<PlayerInfo> connectedPlayers
        {
            get
            {
                List<PlayerInfo> list = new List<PlayerInfo>();
                if (SaveManager.HasKey(connectedPlayersKey))
                {
                    string receivedStr = SaveManager.GetString(connectedPlayersKey, "");
                    Debug.Log(receivedStr);
                    list.AddRange(JsonConvert.DeserializeObject<PlayerInfo[]>(receivedStr));
                }
                for (int i = 0; i < list.Count; i++)
                {
                    PlayerInfo info = list[i];
                    info.IsMe = playerLoginId == info.Id;
                    list[i] = info;
                }
                return list;
            }
            set
            {
                List<PlayerInfo> list = value;
                for (int i = 0; i < list.Count; i++)
                {
                    PlayerInfo info = list[i];
                    info.IsMe = false;
                    list[i] = info;
                }
                SaveManager.SaveString(connectedPlayersKey, JsonConvert.SerializeObject(list));
            }
        }
        private List<RoomInfo> activeRooms
        {
            get
            {
                List<RoomInfo> list = new List<RoomInfo>();
                if (SaveManager.HasKey(activeRoomsKey))
                    list.AddRange(JsonConvert.DeserializeObject<RoomInfo[]>(SaveManager.GetString(activeRoomsKey, "")));
                return list;
            }
            set => SaveManager.SaveString(activeRoomsKey, JsonConvert.SerializeObject(value));
        }
        public override bool IsInRoom { get => !string.IsNullOrEmpty(CurrentRoomInfo.Id); }

        private void OnApplicationQuit ()
        {
            LeaveMatch();
            if (connectedPlayers.Count <= 1)
            {
                Debug.Log("[Testing] Cleaning up to quit.");
                SaveManager.DeleteKey(connectedPlayersKey);
                SaveManager.DeleteKey(activeRoomsKey);
            }
        }

        private IEnumerator CheckCoroutine ()
        {
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return wait;
                if (IsInRoom)
                {
                    //Check rooms
                    var roomList = activeRooms;
                    foreach (var room in roomList)
                    {
                        if (room.Id == CurrentRoomInfo.Id)
                        {
                            CurrentRoomInfo = room;
                            break;
                        }
                    }
                    //Check players
                    List<PlayerInfo> currentPlayerList = CurrentRoomInfo.Players;
                    for (int i = 0; i < currentPlayerList.Count || i < lastKnownPlayerList.Count; i++)
                    {
                        if (i < currentPlayerList.Count)
                        {
                            PlayerInfo newPlayer = currentPlayerList[i];
                            if (newPlayer.Id == MyPlayerInfo.Id)
                                MyPlayerInfo = newPlayer;
                            else
                            {
                                PlayerInfo found = lastKnownPlayerList.Find((info) => info.Id == newPlayer.Id);
                                if (string.IsNullOrEmpty(found.Id))
                                    RaisePlayerEnteredRoom(newPlayer);
                                else if (newPlayer.IsMasterClient && !found.IsMasterClient)
                                    RaiseMasterClientChanged(newPlayer);
                            }
                        }
                        if (i < lastKnownPlayerList.Count)
                        {
                            PlayerInfo formerPlayer = lastKnownPlayerList[i];
                            if (formerPlayer.Id != MyPlayerInfo.Id && string.IsNullOrEmpty(currentPlayerList.Find((info) => info.Id == formerPlayer.Id).Id))
                                RaisePlayerLeftRoom(formerPlayer);
                        }
                    }
                    lastKnownPlayerList.Clear();
                    lastKnownPlayerList.AddRange(currentPlayerList);
                    //Check functions
                    // [ChangePlayerStatus-hq3j6ieuna151mn(Master);ccieguya12gh5;schch2h213ghm5][ChangePlayerStatus-k24wdht8ocjdoo(Idle);ccieguya12gh5]
                    string functionBoard = SaveManager.GetString(executeFunctionKey, "");
                    if (!string.IsNullOrEmpty(functionBoard))
                    {
                        string[] split = functionBoard.Split(functionCallSeparator, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in split)
                        {
                            string[] parts = line.Split(functionDataSeparator, StringSplitOptions.RemoveEmptyEntries);
                            if (!line.Substring(line.IndexOf(')')).Contains(playerLoginId))
                            {
                                int indexOfPar = parts[0].IndexOf('(');
                                int indexOfDash = parts[0].IndexOf('-');
                                string functionName = parts[0].Substring(0, indexOfDash);
                                string[] argumentsSerial = parts[0].Substring(indexOfPar + 1).Split(functionArgumentsSeparator, StringSplitOptions.RemoveEmptyEntries);
                                object[] arguments = argumentsSerial != null ? new object[argumentsSerial.Length] : null;
                                if (arguments != null)
                                    for (int i = 0; i < arguments.Length; i++)
                                        arguments[i] = JsonConvert.DeserializeObject(argumentsSerial[i]);
                                RaiseEventReceived(functionName, arguments);
                                string newLine = line;
                                newLine += $";{playerLoginId}";
                                functionBoard.Replace(line, newLine);
                                SaveManager.SaveString(executeFunctionKey, functionBoard);
                            }
                        }
                    }
                }
            }
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
            PlayerInfo info = MyPlayerInfo;
            info.Nickname = name;
            MyPlayerInfo = info;
            SaveManager.SaveNickname(name);
        }

        public override void LogIn (object parameter = null)
        {
            Assert.IsTrue(IsConnected);

            this.Wait(randomTime, () =>
            {
                List<PlayerInfo> listOfPlayers = connectedPlayers;
                bool isFirst = listOfPlayers == null || listOfPlayers.Count == 0;
                playerLoginId = smallGuid;
                PlayerInfo playerInfo = new PlayerInfo()
                {
                    Id = playerLoginId,
                    Nickname = SaveManager.GetNickname(),
                    IsMasterClient = isFirst,
                    IsMe = false,
                };
                MyPlayerInfo = playerInfo;
                listOfPlayers.Add(playerInfo);
                connectedPlayers = listOfPlayers;
                StartCoroutine(CheckCoroutine());
                Debug.Log("[Testing] Logged In");
                RaiseLogInSuccess();
            });
        }

        public override void FindMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsNotNull(parameter);

            if (parameter is RoomOptions)
            {
                RoomOptions options = new RoomOptions();
                options = (RoomOptions)parameter;
                Debug.Log("[Testing] Creating Room");
                this.Wait(randomTime, () =>
                {
                    RoomInfo newRoom = new RoomInfo()
                    {
                        Id = CreateRandomRoomName(),
                        MaxPlayers = options.MaxPlayers,
                        Players = new List<PlayerInfo>() { MyPlayerInfo },
                        PlayerCount = 1
                    };
                    CurrentRoomInfo = newRoom;
                    List<RoomInfo> roomList = activeRooms;
                    roomList.Add(newRoom);
                    activeRooms = roomList;
                    RaiseFindMatchSuccess();
                    RaisePlayerEnteredRoom(MyPlayerInfo);
                    Debug.Log($"[Testing] Room created: {newRoom.Id}");
                });
            }
            else if (parameter is string)
            {
                this.Wait(randomTime, () =>
                {
                    string wantedRoom = (string)parameter;
                    List<RoomInfo> roomList = activeRooms;
                    int foundIndex = -1;
                    for (int i = 0; i < roomList.Count; i++)
                    {
                        if (roomList[i].Id == wantedRoom)
                        {
                            foundIndex = i;
                            break;
                        }
                    }
                    if (foundIndex >= 0)
                    {
                        RoomInfo room = roomList[foundIndex];
                        List<PlayerInfo> playersInRoom = room.Players;
                        playersInRoom.Add(MyPlayerInfo);
                        room.Players = playersInRoom;
                        room.PlayerCount = roomList.Count;
                        roomList[foundIndex] = room;
                        CurrentRoomInfo = room;
                        activeRooms = roomList;
                        RaiseFindMatchSuccess();
                        RaisePlayerEnteredRoom(MyPlayerInfo);
                        Debug.Log($"[Testing] Entering room: {room.Id}");
                    }
                    else
                    {
                        RaiseFindMatchFailure();
                        Debug.Log($"[Testing] Room not found: {wantedRoom}");
                    }
                });
            }
        }

        public override void LeaveMatch (object parameter = null)
        {
            Assert.IsTrue(IsConnected);
            Assert.IsTrue(IsInRoom);

            List<RoomInfo> listOfRooms = activeRooms;
            for (int i = 0; i < listOfRooms.Count; i++)
            {
                RoomInfo room = listOfRooms[i];
                if (room.Id == CurrentRoomInfo.Id)
                {
                    List<PlayerInfo> players = room.Players;
                    for (int j = players.Count - 1; j >= 0; j--)
                    {
                        if (players[j].Id == MyPlayerInfo.Id)
                        {
                            players.RemoveAt(j);
                            RaisePlayerLeftRoom(MyPlayerInfo);
                            if (MyPlayerInfo.IsMasterClient)
                            {
                                PlayerInfo myInfo = MyPlayerInfo;
                                myInfo.IsMasterClient = false;
                                MyPlayerInfo = myInfo;
                            }
                            if (players.Count > 0)
                            {
                                PlayerInfo newMasterInfo = players[0];
                                newMasterInfo.IsMasterClient = true;
                                players[0] = newMasterInfo;
                                RaiseMasterClientChanged(newMasterInfo);
                            }
                            break;
                        }
                    }
                    room.Players = players;
                    room.PlayerCount = players.Count;
                    listOfRooms[i] = room;
                    if (players.Count == 0)
                    {
                        listOfRooms.RemoveAt(i);
                        SaveManager.DeleteKey(executeFunctionKey);
                    }
                    break;
                }
            }
            activeRooms = listOfRooms;
            CurrentRoomInfo = new RoomInfo();
            Debug.Log("[Testing] Left Room.");
        }

        public override void SendData (params object[] parameters)
        {
            Assert.IsTrue(IsConnected);
            Dictionary<string, object> paramDict = parameters.ToDictionary();
            List<object> successKeys = new List<object>();
            foreach (var item in paramDict)
            {
                SaveManager.SaveString(item.Key, JsonConvert.SerializeObject(item.Value));
                successKeys.Add(item.Key);
            }
            this.Wait(randomTime, () => RaiseSendDataSuccess(successKeys.ToArray()));
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

            string parametersSerialization = "";
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i > 0)
                        parametersSerialization += ",";
                    parametersSerialization += JsonConvert.SerializeObject(parameters[i]);
                }
            }
            string newBoard = SaveManager.GetString(executeFunctionKey, "");
            newBoard += $"[{eventKey}-{DateTime.Now.ToBinary()}({parametersSerialization})]";
            SaveManager.SaveString(executeFunctionKey, newBoard);
        }
    }
}
