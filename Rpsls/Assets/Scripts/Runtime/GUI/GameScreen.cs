using System;
using System.Collections;
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
        [SerializeField, ChildGameObjectsOnly] private GridLayoutGroup playerSlotsListParent;
        [SerializeField, ChildGameObjectsOnly] private Transform[] versusPositions;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;

        private RpslsGameSettings settings;
        private string myId;
        private string currentOpponentId;
        private Dictionary<string, PlayerInfoSlot> playerSlots = new Dictionary<string, PlayerInfoSlot>();
        private TournamentInfo tournamentInfo;
        private MatchInfo myMatch;

        private void Awake ()
        {
            GameManagerClient.OnPlayerListReceived += HandlePlayerListReceived;
            GameManagerClient.OnTournamentUpdated += HandleTournamentUpdated;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            settings = RpslsGameSettings.Instance;
            myId = NetworkManager.Instance.MyPlayerInfo.Id;
        }

        private void OnDestroy ()
        {
            GameManagerClient.OnPlayerListReceived -= HandlePlayerListReceived;
            GameManagerClient.OnTournamentUpdated -= HandleTournamentUpdated;
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void Start ()
        {
            StartCoroutine(PlayersEntryAnimation());
        }

        private void HandlePlayerListReceived (List<PlayerInfo> receivedPlayers)
        {
            if (receivedPlayers.Count == 2 && receivedPlayers[0].Id == myId)
            {
                PlayerInfo myInfo = receivedPlayers[0];
                receivedPlayers.Remove(myInfo);
                receivedPlayers.Add(myInfo);
            }
            for (int i = 0; i < receivedPlayers.Count; i++)
            {
                PlayerInfoSlot slot = CreateSlot(receivedPlayers[i]);
                playerSlots.Add(receivedPlayers[i].Id, slot);
                slot.transform.localScale = 0.01f * Vector3.one;
            }
        }

        private void HandleTournamentUpdated (TournamentInfo tournamentInfo)
        {
            this.tournamentInfo = tournamentInfo;
            foreach (var item in tournamentInfo.Matches)
            {
                if (item.Player1.Id == myId)
                {
                    myMatch = item;
                    currentOpponentId = item.Player2?.Id;
                }
                else if (item.Player2 != null && item.Player2.Id == myId)
                {
                    myMatch = item;
                    currentOpponentId = item.Player1.Id;
                }
            }
        }

        private IEnumerator PlayersEntryAnimation ()
        {
            // Wait 2 seconds
            yield return new WaitForSeconds(2f);
            // Wait for player slots being filled
            while (playerSlots.Count == 0)
                yield return null;
            // Present each player slot
            float delay = 0.1f;
            float currentDelay = delay;
            foreach (var item in playerSlots)
            {
                Transform itemTransf = item.Value.transform;
                itemTransf.DOScale(1, 0.3f).SetEase(Ease.InCubic).SetDelay(currentDelay += delay).OnComplete(() =>
                    itemTransf.DOPunchScale(0.1f * Vector3.one, 0.2f, 1));
            }
            yield return new WaitForSeconds(currentDelay + 1f);
            playerSlotsListParent.enabled = false;
            foreach (var item in playerSlots)
                item.Value.transform.SetParent(playerSlotsListParent.transform.parent);
            // Go to tournament presentation
            StartCoroutine(TournamentStartAnimation());
        }

        private IEnumerator TournamentStartAnimation ()
        {
            // Wait for tournament info
            while (tournamentInfo == null)
                yield return null;
            // Set the BYE player badge, if any
            if (tournamentInfo.Matches.Length > 1)
            {
                // Build tournament structure
                // Move tournament structure to view
                //      also Move each player slot to its position with a jump
                //      also Pan the bottom of the tournament matches
                // Move out to show the entire structure
            }
            StartCoroutine(MatchStartAnimation());
        }

        private IEnumerator MatchStartAnimation ()
        {
            // Hide tournament structure
            if (string.IsNullOrEmpty(currentOpponentId))
            {
                // Move my slot to the bottom of the screen
                //      also Move each other battle to the center of the screen larger than usual
            }
            else
            {
                // Move my opponent and mine slots to the center of the screen BIG
                //      also Move each other player to the sides of the screen (small) according to their matches
                Transform mySlot = playerSlots[myId].transform;
                Transform opponentSlot = playerSlots[currentOpponentId].transform;
                float moveToVersusTime = 2f;
                opponentSlot.DOMove(versusPositions[0].position, moveToVersusTime);
                opponentSlot.DOScale(versusPositions[0].localScale, moveToVersusTime);
                mySlot.DOMove(versusPositions[1].position, moveToVersusTime);
                mySlot.DOScale(versusPositions[1].localScale, moveToVersusTime);
                yield return new WaitForSeconds(moveToVersusTime);
                // Play a versus animation
                // Dock my opponent slot at the top of the screen and mine at the bottom
                // Play the Playmat intro animation (roll the playmat?)
                // Bring in the cards to their positions
            }
            StartCoroutine(MatchCoroutine());
        }

        private IEnumerator MatchCoroutine ()
        {
            // Wait for hand
            // Turn routine
            yield return null;
        }

        private void OnExitButtonClicked ()
        {
            GameManagerClient.ExitRoom();
        }

        private PlayerInfoSlot CreateSlot (PlayerInfo info)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.GameInfoSlotPrefab, playerSlotsListParent.transform);
            newSlot.HandlePlayerInfo(info);
            return newSlot;
        }
    }
}
