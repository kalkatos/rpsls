using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Kalkatos.Network;

namespace Kalkatos.Tournament
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
        [SerializeField, ChildGameObjectsOnly] private Button copyRoomIdButton;
        [SerializeField] private UnityEvent onNotEveryoneReady;
        [SerializeField] private UnityEvent onNotEnoughPlayers;

        private TournamentGameSettings settings;
        private Dictionary<string, PlayerInfoSlot> slots = new Dictionary<string, PlayerInfoSlot>();
        private bool isReady;

        private void Awake ()
        {
            RoomManager.OnPlayerListReceived += HandlePlayerListReceived;
            RoomManager.OnPlayerEntered+= HandlePlayerEntered;
            RoomManager.OnPlayerLeft += HandlePlayerLeft;
            RoomManager.OnPlayerInfoChanged += HandlePlayerInfoChanged;
            RoomManager.OnBecameMaster += HandleBecameMaster;
            RoomManager.OnGameAboutToStart += HandleGameAboutToStart;
            RoomManager.OnGameNotReady += HandleGameNotReady;
            idleButton.onClick.AddListener(ClickedOnIdleButton);
            readyButton.onClick.AddListener(ClickedOnReadyButton);
            startGameButton.onClick.AddListener(ClickedOnStartGameButton);
            exitButton.onClick.AddListener(ClickedOnExitButton);
            copyRoomIdButton.onClick.AddListener(ClickedOnCopyRoomIdButton);

            settings = TournamentGameSettings.Instance;
        }

        private void OnDestroy ()
        {
            RoomManager.OnPlayerListReceived -= HandlePlayerListReceived;
            RoomManager.OnPlayerEntered -= HandlePlayerEntered;
            RoomManager.OnPlayerLeft -= HandlePlayerLeft;
            RoomManager.OnPlayerInfoChanged -= HandlePlayerInfoChanged;
            RoomManager.OnBecameMaster -= HandleBecameMaster;
            RoomManager.OnGameAboutToStart -= HandleGameAboutToStart;
            RoomManager.OnGameNotReady -= HandleGameNotReady;
            idleButton.onClick.RemoveListener(ClickedOnIdleButton);
            readyButton.onClick.RemoveListener(ClickedOnReadyButton);
            startGameButton.onClick.RemoveListener(ClickedOnStartGameButton);
            exitButton.onClick.RemoveListener(ClickedOnExitButton);
            copyRoomIdButton.onClick.RemoveListener(ClickedOnCopyRoomIdButton);
        }

        private void Start ()
        {
            SetRoomName();
            SetupButtons();
        }

        private void HandlePlayerListReceived (List<PlayerInfo> list)
        {
            slots.Clear();
            for (int i = 0; i < list.Count; i++)
                slots.Add(list[i].Id, CreateSlot(list[i]));
            UpdatePlayerCount();
        }

        private void HandlePlayerEntered (PlayerInfo newPlayer)
        {
            if (!slots.ContainsKey(newPlayer.Id))
                slots.Add(newPlayer.Id, CreateSlot(newPlayer));
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

        private void HandlePlayerInfoChanged (PlayerInfo info)
        {
            if (slots.TryGetValue(info.Id, out PlayerInfoSlot slot))
                slot.HandlePlayerInfo(info, "");
        }

        private void HandleBecameMaster ()
        {
            SetupButtons();
        }

        private void HandleGameAboutToStart ()
        {
            idleButton.interactable = false;
            readyButton.interactable = false;
            startGameButton.interactable = false;
        }

        private void HandleGameNotReady (NotReadyErrorCode errorCode)
        {
            switch (errorCode)
            {
                case NotReadyErrorCode.NotEnoughPlayers:
                    onNotEnoughPlayers?.Invoke();
                    break;
                case NotReadyErrorCode.NotEveryoneReady:
                    onNotEveryoneReady?.Invoke();
                    break;
            }
        }

        private void ClickedOnIdleButton ()
        {
            RoomManager.SetStatus(RoomStatus.Ready);
            isReady = true;
            SetupButtons();
        }

        private void ClickedOnReadyButton ()
        {
            RoomManager.SetStatus(RoomStatus.Idle);
            isReady = false;
            SetupButtons();
        }

        private void ClickedOnStartGameButton ()
        {
            RoomManager.StartGame();
        }

        private void ClickedOnExitButton ()
        {
            RoomManager.ExitRoom();
        }

        private void ClickedOnCopyRoomIdButton ()
        {
            GUIUtility.systemCopyBuffer = RoomManager.RoomName;
        }

        private void SetupButtons ()
        {
            bool isMaster = RoomManager.IAmTheMaster;
            idleButton.gameObject.SetActive(!isMaster && !isReady);
            readyButton.gameObject.SetActive(!isMaster && isReady);
            startGameButton.gameObject.SetActive(isMaster);
        }

        private void UpdatePlayerCount ()
        {
            playerCountText.text = $"{slots.Count} / {settings.MaxPlayers}";
        }

        private void SetRoomName ()
        {
            roomNameText.text = "Room: " + RoomManager.RoomName;
        }

        private PlayerInfoSlot CreateSlot (PlayerInfo info)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.RoomInfoSlotPrefab, playerSlotsListParent);
            newSlot.HandlePlayerInfo(info, "");
            return newSlot;
        }
    }
}
