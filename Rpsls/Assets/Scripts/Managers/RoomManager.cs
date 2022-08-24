using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Kalkatos.Rpsls
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static event Action<List<Player>> OnPlayerListReceived;
        public static event Action<Player> OnPlayerEntered;
        public static event Action<Player> OnPlayerLeft;

        public List<Player> players = new List<Player>();

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
    }
}
