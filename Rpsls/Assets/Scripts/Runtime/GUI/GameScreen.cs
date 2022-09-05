using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Kalkatos.Network;
using DG.Tweening;

namespace Kalkatos.Rpsls
{
    public class GameScreen : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly] private Transform playerSlotsListParent;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;

        private RpslsGameSettings settings;
        private Dictionary<string, PlayerInfoSlot> playerSlots = new Dictionary<string, PlayerInfoSlot>();

        private void Awake ()
        {
            GameManagerClient.OnPlayerListReceived += HandlePlayerListReceived;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            settings = RpslsGameSettings.Instance;
        }

        private void OnDestroy ()
        {
            GameManagerClient.OnPlayerListReceived -= HandlePlayerListReceived;
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void HandlePlayerListReceived (List<PlayerInfo> receivedPlayers)
        {
            for (int i = 0; i < receivedPlayers.Count; i++)
            {
                PlayerInfoSlot slot = CreateSlot(receivedPlayers[i]);
                playerSlots.Add(receivedPlayers[i].Id, slot);
                slot.transform.DOPunchScale(1.2f * Vector3.one, 0.3f, 1);
            }
        }

        private void OnExitButtonClicked ()
        {
            GameManagerClient.ExitRoom();
        }

        private PlayerInfoSlot CreateSlot (PlayerInfo info)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.GameInfoSlotPrefab, playerSlotsListParent);
            newSlot.HandlePlayerInfo(info);
            return newSlot;
        }
    }
}
