using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace Kalkatos.Rpsls
{
    public class RoomScreen : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly] private Transform playerSlotsListParent;
        [SerializeField, ChildGameObjectsOnly] private TMP_Text roomNameText;
        [SerializeField, ChildGameObjectsOnly] private TMP_Text playerCountText;
        [SerializeField, ChildGameObjectsOnly] private Button idleButton;
        [SerializeField, ChildGameObjectsOnly] private Button readyButton;
        [SerializeField, ChildGameObjectsOnly] private Button startGameButton;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;

        private Settings settings;
        private Dictionary<string, PlayerInfoSlot> slots = new Dictionary<string, PlayerInfoSlot>();
        private bool isReady;

        private void Awake ()
        {
            RoomManager.OnPlayerListReceived += HandlePlayerListReceived;
            RoomManager.OnPlayerEntered+= HandlePlayerEntered;
            RoomManager.OnPlayerLeft += HandlePlayerLeft;
            RoomManager.OnPlayerStatusChanged += HandlePlayerStatusChanged;
            idleButton.onClick.AddListener(HandleIdleButtonClicked);
            readyButton.onClick.AddListener(HandleReadyButtonClicked);
            startGameButton.onClick.AddListener(HandleStartGameButtonClicked);
            exitButton.onClick.AddListener(HandleExitButtonClicked);

            settings = Resources.Load<Settings>("RpslsGameSettings");
        }

        private void OnDestroy ()
        {
            RoomManager.OnPlayerListReceived -= HandlePlayerListReceived;
            RoomManager.OnPlayerEntered -= HandlePlayerEntered;
            RoomManager.OnPlayerLeft -= HandlePlayerLeft;
            RoomManager.OnPlayerStatusChanged -= HandlePlayerStatusChanged;
            idleButton.onClick.RemoveListener(HandleIdleButtonClicked);
            readyButton.onClick.RemoveListener(HandleReadyButtonClicked);
            startGameButton.onClick.RemoveListener(HandleStartGameButtonClicked);
            exitButton.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void Start ()
        {
            SetRoomName(RoomManager.RoomName);
            SetupButtons(RoomManager.IAmTheMaster);
        }

        private void HandlePlayerListReceived (List<PlayerInfo> list)
        {
            slots.Clear();
            for (int i = 0; i < list.Count; i++)
                slots.Add(list[i].Id, CreateSlot(list[i].Nickname, list[i].IsMasterClient));
            UpdatePlayerCount();
        }

        private void HandlePlayerEntered (PlayerInfo newPlayer)
        {
            slots.Add(newPlayer.Id, CreateSlot(newPlayer.Nickname, newPlayer.IsMasterClient));
            UpdatePlayerCount();
        }

        private void HandlePlayerLeft (PlayerInfo player)
        {
            if (slots.ContainsKey(player.Id))
            {
                Destroy(slots[player.Id].gameObject);
                slots.Remove(player.Id);
            }
            UpdatePlayerCount();
        }

        private void HandlePlayerStatusChanged (string userId, RoomStatus status)
        {
            if (slots.TryGetValue(userId, out PlayerInfoSlot slot))
                slot.SetStatus(status);
        }

        private void HandleIdleButtonClicked ()
        {
            RoomManager.SetStatus(RoomStatus.Ready);
        }

        private void HandleReadyButtonClicked ()
        {
            RoomManager.SetStatus(RoomStatus.Idle);
        }

        private void HandleStartGameButtonClicked ()
        {
            RoomManager.StartGame();
        }

        private void HandleExitButtonClicked ()
        {
            RoomManager.ExitRoom();
        }

        private void SetupButtons (bool isMaster)
        {
            idleButton.gameObject.SetActive(!isMaster && !isReady);
            readyButton.gameObject.SetActive(!isMaster && isReady);
            startGameButton.gameObject.SetActive(isMaster);
        }

        private void UpdatePlayerCount ()
        {
            playerCountText.text = $"{slots.Count} / {settings.MaxPlayers}";
        }

        private void SetRoomName (string name)
        {
            roomNameText.text = "Room: " + name;
        }

        private PlayerInfoSlot CreateSlot (string nick, bool isMaster)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.PlayerInfoSlotPrefab, playerSlotsListParent);
            newSlot.SetNickname(nick);
            newSlot.SetStatus(isMaster ? RoomStatus.Master : RoomStatus.Idle);
            return newSlot;
        }
    }
}
