using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System;

namespace Kalkatos.Rpsls
{
    public class RoomScreen : MonoBehaviour
    {
        [SerializeField] private PlayerInfoSlot slotPrefab;
        [SerializeField] private Button getReadyButton;
        [SerializeField] private Button startGameButton;

        private Dictionary<string, PlayerInfoSlot> slots = new Dictionary<string, PlayerInfoSlot>();

        private void Awake ()
        {
            RoomManager.OnPlayerListReceived += HandlePlayerListReceived;
            RoomManager.OnPlayerEntered+= HandlePlayerEntered;
            RoomManager.OnPlayerLeft += HandlePlayerLeft;
        }

        private void OnDestroy ()
        {
            RoomManager.OnPlayerListReceived -= HandlePlayerListReceived;
            RoomManager.OnPlayerEntered -= HandlePlayerEntered;
            RoomManager.OnPlayerLeft -= HandlePlayerLeft;
        }

        private void HandlePlayerListReceived (List<Player> list)
        {
            slots.Clear();
            for (int i = 0; i < list.Count; i++)
                slots.Add(list[i].UserId, CreateSlot(list[i].NickName, list[i].IsMasterClient));
        }

        private void HandlePlayerEntered (Player newPlayer)
        {
            slots.Add(newPlayer.UserId, CreateSlot(newPlayer.NickName, newPlayer.IsMasterClient));
        }

        private void HandlePlayerLeft (Player player)
        {
            if (slots.ContainsKey(player.UserId))
            {
                Destroy(slots[player.UserId].gameObject);
                slots.Remove(player.UserId);
            }
        }

        private PlayerInfoSlot CreateSlot (string nick, bool isMaster)
        {
            PlayerInfoSlot newSlot = Instantiate(slotPrefab);
            newSlot.SetNickname(nick);
            newSlot.SetStatus(isMaster ? RoomStatus.Master : RoomStatus.Idle);
            return newSlot;
        }
    }
}
