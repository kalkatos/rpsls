using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Kalkatos.Rpsls
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager Instance { get; private set; }

        public static event Action<List<PlayerInfo>> OnPlayerListReceived;
        public static event Action<PlayerInfo> OnPlayerEntered;
        public static event Action<PlayerInfo> OnPlayerLeft;
        public static event Action<string, RoomStatus> OnPlayerStatusChanged;
        public static event Action OnBecameMaster;

        public List<Player> players = new List<Player>();

        public static string RoomName { get; private set; }
        public static bool IAmTheMaster { get; private set; }

        private void Awake ()
        {
            Instance = this;
            RoomName = PhotonNetwork.CurrentRoom.Name;
            IAmTheMaster = PhotonNetwork.IsMasterClient;
        }

        private void Start ()
        {
            players.AddRange(PhotonNetwork.PlayerList);
            List<PlayerInfo> infos = new List<PlayerInfo>();
            for (int i = 0; i < players.Count; i++)
                infos.Add(PlayerInfo.From(players[i]));
            OnPlayerListReceived?.Invoke(infos);
            SetStatus(IAmTheMaster ? RoomStatus.Master : RoomStatus.Idle);
        }

        public override void OnPlayerEnteredRoom (Player newPlayer)
        {
            players.Add(newPlayer);
            OnPlayerEntered?.Invoke(PlayerInfo.From(newPlayer));
        }

        public override void OnPlayerLeftRoom (Player otherPlayer)
        {
            players.Remove(otherPlayer);
            OnPlayerLeft?.Invoke(PlayerInfo.From(otherPlayer));
        }

        public override void OnMasterClientSwitched (Player newMasterClient)
        {
            if (newMasterClient.IsLocal)
            {
                SetStatus(RoomStatus.Master);
                IAmTheMaster = true;
                OnBecameMaster?.Invoke();
            }
        }

        public override void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey("RoomStatus"))
                OnPlayerStatusChanged?.Invoke(targetPlayer.UserId, (RoomStatus)changedProps["RoomStatus"]);
        }

        public static void SetStatus (RoomStatus status)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "RoomStatus", status } });
        }

        public static void StartGame ()
        {
            SceneManager.EndScene("ToGame");
        }

        public static void ExitRoom ()
        {
            RoomName = "";
            IAmTheMaster = false;
            SceneManager.EndScene("ToLobby");
            PhotonNetwork.LeaveRoom();
        }
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string Id;
        public string Nickname;
        public bool IsMasterClient;
        public bool IsMe;
        public RoomStatus Status;

        public static PlayerInfo From (Player player)
        {
            return new PlayerInfo(player);
        }

        public PlayerInfo (Player player)
        {
            Id = player.UserId;
            Nickname = player.NickName;
            IsMasterClient = player.IsMasterClient;
            IsMe = player.IsLocal;
            if (player.CustomProperties.ContainsKey("RoomStatus"))
                Status = (RoomStatus)player.CustomProperties["RoomStatus"];
            else
                Status = player.IsMasterClient ? RoomStatus.Master : RoomStatus.Idle;
        }
    }
}
