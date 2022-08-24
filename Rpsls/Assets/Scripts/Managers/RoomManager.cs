using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Kalkatos.Rpsls
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager Instance { get; private set; }

        public static event Action<List<Player>> OnPlayerListReceived;
        public static event Action<Player> OnPlayerEntered;
        public static event Action<Player> OnPlayerLeft;
        public static event Action<string, RoomStatus> OnPlayerStatusChanged;

        public List<Player> players = new List<Player>();

        private void Awake ()
        {
            Instance = this;
        }

        private void Start ()
        {
            players.AddRange(PhotonNetwork.PlayerList);
            OnPlayerListReceived?.Invoke(players);
        }

        public override void OnPlayerEnteredRoom (Player newPlayer)
        {
            players.Add(newPlayer);
            OnPlayerEntered?.Invoke(newPlayer);
        }

        public override void OnPlayerLeftRoom (Player otherPlayer)
        {
            players.Remove(otherPlayer);
            OnPlayerLeft?.Invoke(otherPlayer);
        }

        public static void StartGame ()
        {
            Debug.LogError("Start Game not implemented.");
        }

        public static void LeaveRoom ()
        {
            Debug.LogError("Leave Room not implemented.");
        }

        [PunRPC]
        public static void SetAsReady (Player player)
        {
            if (!player.IsMasterClient)
                OnPlayerStatusChanged?.Invoke(player.UserId, RoomStatus.Ready);
            //TODO Finish Ready RPCs
        }

        [PunRPC]
        public static void SetAsNotReady (Player player)
        {
            if (!player.IsMasterClient)
                OnPlayerStatusChanged?.Invoke(player.UserId, RoomStatus.Idle);
        }
    }
}
