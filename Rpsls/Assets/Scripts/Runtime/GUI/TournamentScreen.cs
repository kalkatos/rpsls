using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Kalkatos.Network;
using DG.Tweening;

namespace Kalkatos.Tournament
{
    public class TournamentScreen : MonoBehaviour
    {
        public static event Action OnTournamentIntroFinished;

        [Header("References")]
        [SerializeField, ChildGameObjectsOnly] private GridLayoutGroup playerSlotsListParent;
        [SerializeField, ChildGameObjectsOnly] private GridLayoutGroup tournamentMatchesParent;
        [SerializeField] private Transform tournamentStructure;
        
        [Header("Positions")]
        [SerializeField] private Transform[] versusPositions;
        [SerializeField] private Transform[] battlePositions;
        [SerializeField] private Transform[] sidePositions;
        [SerializeField] private Transform[] centerPositions;
        [SerializeField] private Transform[] matchesPositions;
        [SerializeField] private Transform[] tournamentPositions;
        [Header("Config")]
        [SerializeField] private TournamentGameSettings settings;

        private string myId;
        private int myMatchIndex;
        private string currentOpponentId;
        private Dictionary<string, PlayerInfoSlot> playerSlots = new Dictionary<string, PlayerInfoSlot>();
        private TournamentInfo tournamentInfo;
        private MatchInfo myMatch;
        private Vector3 tournamentHiddenPosition;

        private void Awake ()
        {
            GameManagerClient.OnPlayerListReceived += HandlePlayerListReceived;
            GameManagerClient.OnTournamentUpdated += HandleTournamentUpdated;
            settings = TournamentGameSettings.Instance;
            myId = NetworkManager.Instance.MyPlayerInfo.Id;
            tournamentHiddenPosition = tournamentStructure.localPosition;
        }

        private void OnDestroy ()
        {
            GameManagerClient.OnPlayerListReceived -= HandlePlayerListReceived;
            GameManagerClient.OnTournamentUpdated -= HandleTournamentUpdated;
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
            int index = 0;
            foreach (var item in tournamentInfo.Matches)
            {
                if (item.Player1.Id == myId)
                {
                    myMatchIndex = index;
                    myMatch = item;
                    currentOpponentId = item.Player2?.Id;
                }
                else if (item.Player2 != null && item.Player2.Id == myId)
                {
                    myMatchIndex = index;
                    myMatch = item;
                    currentOpponentId = item.Player1.Id;
                }
                index++;
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
                tournamentMatchesParent.enabled = false;
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
                for (int i = 0; i < matchesPositions.Length; i++)
                {
                    if (i == myMatchIndex)
                    {
                        // Move my opponent and mine slots to the center of the screen BIG
                        matchesPositions[i].gameObject.SetActive(false);
                        MoveAndScaleTo(playerSlots[myId].transform, versusPositions[1], moveToVersusTime);
                        MoveAndScaleTo(playerSlots[currentOpponentId].transform, versusPositions[0], moveToVersusTime);
                    }
                    else
                    {
                        MoveAndScaleTo(matchesPositions[i], sidePositions[sidePositionsIndex++], moveToVersusTime);
                        //      also Move each other player to the sides of the screen (small) according to their matches
                        //MoveAndScaleTo(playerSlots[item.Player1.Id].transform, sidePositions[sidePositionsIndex++], moveToVersusTime);
                        //if (item.Player2 != null)
                        //    MoveAndScaleTo(playerSlots[item.Player2.Id].transform, sidePositions[sidePositionsIndex++], moveToVersusTime);
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
            }
            OnTournamentIntroFinished?.Invoke();
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

        private PlayerInfoSlot CreateSlot (PlayerInfo info)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.GameInfoSlotPrefab, playerSlotsListParent.transform);
            newSlot.HandlePlayerInfo(info);
            return newSlot;
        }
    }
}
