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
        [Header("References")]
        [SerializeField, ChildGameObjectsOnly] private GridLayoutGroup playerSlotsListParent;
        [SerializeField] private GameObject timerBarObj;
        [SerializeField] private RectTransform timerBar;
        [SerializeField] private Transform tournamentStructure;
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;
        [Header("Positions")]
        [SerializeField] private Transform[] versusPositions;
        [SerializeField] private Transform[] battlePositions;
        [SerializeField] private Transform[] sidePositions;
        [SerializeField] private Transform[] centerPositions;
        [SerializeField] private Transform[] playmatPositions;
        [SerializeField] private Transform[] matchesPositions;
        [SerializeField] private Transform[] tournamentPositions;
        [Header("Config")]
        [SerializeField] private RpslsGameSettings settings;

        private string myId;
        private string currentOpponentId;
        private Dictionary<string, PlayerInfoSlot> playerSlots = new Dictionary<string, PlayerInfoSlot>();
        private TournamentInfo tournamentInfo;
        private MatchInfo myMatch;
        private Transform[] playmats;
        private Vector3 tournamentHiddenPosition;

        private void Awake ()
        {
            GameManagerClient.OnPlayerListReceived += HandlePlayerListReceived;
            GameManagerClient.OnTournamentUpdated += HandleTournamentUpdated;
            exitButton.onClick.AddListener(OnExitButtonClicked);
            settings = RpslsGameSettings.Instance;
            myId = NetworkManager.Instance.MyPlayerInfo.Id;
            tournamentHiddenPosition = tournamentStructure.localPosition;
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

        #region ================= Animations ========================

        private IEnumerator PlayersEntryAnimation ()
        {
            // TODO Play an intro animation

            //yield return new WaitForSeconds(settings.IntroAnimationTime);
            // Wait for player slots being filled
            while (playerSlots.Count == 0)
                yield return null;
            // Present each player slot
            float delay = settings.DelayBetweenSlotsAppearing;
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
                // Build tournament structure
                float moveToTournamentTime = settings.MoveToTournamentTime;
                for (int i = 0; i < matchesPositions.Length; i++)
                    matchesPositions[i].gameObject.SetActive(i < tournamentInfo.Matches.Length);
                int index = 0;
                // Move each player slot to its position with a jump
                foreach (var item in tournamentInfo.Matches)
                {
                    string p1 = item.Player1.Id;
                    string p2 = item.Player2?.Id ?? "";
                    MoveAndScaleTo(playerSlots[p1].transform, tournamentPositions[index++], moveToTournamentTime, true);
                    int p2Index = index++;
                    if (!string.IsNullOrEmpty(p2))
                        MoveAndScaleTo(playerSlots[p2].transform, tournamentPositions[p2Index], moveToTournamentTime, true);
                }
                //  Move tournament structure to view
                tournamentStructure.DOLocalMove(Vector3.zero, moveToTournamentTime / 2f);
                yield return new WaitForSeconds(moveToTournamentTime + settings.TournamentShowTime);
                // Hide tournament structure
                tournamentStructure.DOLocalMove(tournamentHiddenPosition, moveToTournamentTime / 2f);
            }
            StartCoroutine(MatchStartAnimation());
        }

        private IEnumerator MatchStartAnimation ()
        {
            bool haveOpponent = !string.IsNullOrEmpty(currentOpponentId);
            if (haveOpponent)
            {
                float moveToVersusTime = settings.MoveToVersusTime;
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
                float versusAnimationTime = settings.VersusAnimationTime;

                yield return new WaitForSeconds(moveToVersusTime + versusAnimationTime);
            }
            float dockingTime = settings.DockingTime;
            // Dock my opponent slot at the top of the screen and mine at the bottom
            if (haveOpponent)
                MoveAndScaleTo(playerSlots[currentOpponentId].transform, battlePositions[0], dockingTime);
            MoveAndScaleTo(playerSlots[myId].transform, battlePositions[1], dockingTime);
            if (!haveOpponent)
            {
                // If I am bye, move opponents' battles to the center of the screen
                int centerPositionsIndex = 0;
                foreach (var item in tournamentInfo.Matches)
                {
                    if (item == myMatch)
                        continue;
                    MoveAndScaleTo(playerSlots[item.Player1.Id].transform, centerPositions[centerPositionsIndex++], dockingTime);
                    MoveAndScaleTo(playerSlots[item.Player2.Id].transform, centerPositions[centerPositionsIndex++], dockingTime);
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

        #endregion

        private void MoveAndScaleTo (Transform obj, Transform destination, float time, bool jump = false)
        {
            obj.SetParent(destination);
            if (jump)
                obj.DOLocalJump(Vector3.zero, 2, 1, time);
            else
                obj.DOLocalMove(Vector3.zero, time);
            obj.DOScale(Vector3.one, time);
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
