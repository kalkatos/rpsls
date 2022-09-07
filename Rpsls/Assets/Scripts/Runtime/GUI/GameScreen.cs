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
        [SerializeField] private Transform[] versusPositions;
        [SerializeField] private Transform[] battlePositions;
        [SerializeField] private Transform[] sidePositions;
        [SerializeField] private Transform[] centerPositions;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;
        [SerializeField] private RpslsGameSettings settings;

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

#if UNITY_EDITOR
        [Button]
        private void AddSlotsForTesting ()
        {
            for (int i = 0; i < versusPositions.Length; i++)
                UnityEditor.PrefabUtility.InstantiatePrefab(settings.GameInfoSlotPrefab, versusPositions[i]);
            for (int i = 0; i < battlePositions.Length; i++)
                UnityEditor.PrefabUtility.InstantiatePrefab(settings.GameInfoSlotPrefab, battlePositions[i]);
            for (int i = 0; i < sidePositions.Length; i++)
                UnityEditor.PrefabUtility.InstantiatePrefab(settings.GameInfoSlotPrefab, sidePositions[i]);
            for (int i = 0; i < centerPositions.Length; i++)
                UnityEditor.PrefabUtility.InstantiatePrefab(settings.GameInfoSlotPrefab, centerPositions[i]);
        }

        [Button]
        private void RemoveSlotsForTesting ()
        {
            for (int i = 0; i < versusPositions.Length && versusPositions[i].childCount > 0; i++)
                DestroyImmediate(versusPositions[i].GetChild(0).gameObject);
            for (int i = 0; i < battlePositions.Length && battlePositions[i].childCount > 0; i++)
                DestroyImmediate(battlePositions[i].GetChild(0).gameObject);
            for (int i = 0; i < sidePositions.Length && sidePositions[i].childCount > 0; i++)
                DestroyImmediate(sidePositions[i].GetChild(0).gameObject);
            for (int i = 0; i < centerPositions.Length && centerPositions[i].childCount > 0; i++)
                DestroyImmediate(centerPositions[i].GetChild(0).gameObject);
        }
#endif

        private IEnumerator PlayersEntryAnimation ()
        {
            // TODO Play an intro animation

            //yield return new WaitForSeconds(2f);
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
            // TODO Set the BYE player badge, if any
            if (tournamentInfo.Matches.Length > 1)
            {
                // TODO Build tournament structure
                // TODO Move tournament structure to view
                // TODO      also Move each player slot to its position with a jump
                // TODO      also Pan the bottom of the tournament matches
                // TODO Move out to show the entire structure
            }
            StartCoroutine(MatchStartAnimation());
        }

        private IEnumerator MatchStartAnimation ()
        {
            // TODO Hide tournament structure

            bool haveOpponent = !string.IsNullOrEmpty(currentOpponentId);
            if (haveOpponent)
            {
                float moveToVersusTime = 1f;
                int sidePositionsIndex = 0;
                foreach (var item in tournamentInfo.Matches)
                {
                    if (item == myMatch)
                    {
                        // Move my opponent and mine slots to the center of the screen BIG
                        MoveAndScaleTo(playerSlots[myId].transform, versusPositions[1], moveToVersusTime);
                        MoveAndScaleTo(playerSlots[currentOpponentId].transform, versusPositions[0], moveToVersusTime);
                    }
                    else
                    {
                        //      also Move each other player to the sides of the screen (small) according to their matches
                        MoveAndScaleTo(playerSlots[item.Player1.Id].transform, sidePositions[sidePositionsIndex++], moveToVersusTime);
                        if (item.Player2 != null)
                            MoveAndScaleTo(playerSlots[item.Player2.Id].transform, sidePositions[sidePositionsIndex++], moveToVersusTime);
                    }
                }
                // TODO Play a versus animation
                float versusAnimationTime = 2f;

                yield return new WaitForSeconds(moveToVersusTime + versusAnimationTime);
            }
            float prepareTime = 1f;
            // Dock my opponent slot at the top of the screen and mine at the bottom
            if (haveOpponent)
                MoveAndScaleTo(playerSlots[currentOpponentId].transform, battlePositions[0], prepareTime);
            MoveAndScaleTo(playerSlots[myId].transform, battlePositions[1], prepareTime);
            if (!haveOpponent)
            {
                // If I am bye, move opponents' battles to the center of the screen
                int centerPositionsIndex = 0;
                foreach (var item in tournamentInfo.Matches)
                {
                    if (item == myMatch)
                        continue;
                    MoveAndScaleTo(playerSlots[item.Player1.Id].transform, centerPositions[centerPositionsIndex++], prepareTime);
                    MoveAndScaleTo(playerSlots[item.Player2.Id].transform, centerPositions[centerPositionsIndex++], prepareTime);
                }
                // TODO Play a "relax there" animation while waiting
            }
            // TODO Play the Playmat intro animation (roll the playmat?)
            // TODO Bring in the cards to their positions
            StartCoroutine(MatchCoroutine());
        }

        private IEnumerator MatchCoroutine ()
        {
            // TODO Wait for hand
            // TODO Turn routine
            yield return null;
        }

        private void MoveAndScaleTo (Transform obj, Transform destination, float time, bool setParent = true)
        {
            if (setParent)
                obj.SetParent(destination);
            obj.DOMove(destination.position, time);
            obj.DOScale(setParent ? Vector3.one : destination.localScale, time);
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
