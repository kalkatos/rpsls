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
        [SerializeField] private Transform[] matchBubbles;
        [Header("Positions")]
        [SerializeField] private Transform[] versusPositions;
        [SerializeField] private Transform[] battlePositions;
        [SerializeField] private Transform[] sidePositions;
        [SerializeField] private Transform[] centerPositions;
        [SerializeField] private Transform[] matchesPositions;
        [SerializeField] private Transform[] tournamentPositions;
        [SerializeField] private Transform[] playmatPositions;
        [Header("Config")]
        [SerializeField] private TournamentGameSettings settings;

        private string myId;
        private int myMatchIndex;
        private string currentOpponentId;
        private Dictionary<string, PlayerInfoSlot> playerSlots = new Dictionary<string, PlayerInfoSlot>();
        private RoundInfo roundInfo;
        private MatchInfo myMatch;
        private Vector3 tournamentHiddenPosition;
        private Transform[] playmats = new Transform[2];
        private bool isTournamentOver;
        private bool isRoundOver;

        private void Awake ()
        {
            GameManager.OnPlayerListUpdated += HandlePlayerListUpdated;
            GameManager.OnRoundReceived += HandleRoundReceived;
            GameManager.OnTurnResultReceived += HandleTurnResultReceived;
            GameManager.OnStateChanged += HandleStateChanged;
            settings = TournamentGameSettings.Instance;
            tournamentHiddenPosition = tournamentStructure.localPosition;
        }

        private void OnDestroy ()
        {
            GameManager.OnPlayerListUpdated -= HandlePlayerListUpdated;
            GameManager.OnRoundReceived -= HandleRoundReceived;
            GameManager.OnTurnResultReceived -= HandleTurnResultReceived;
            GameManager.OnStateChanged -= HandleStateChanged;
        }

        private void Start ()
        {
            StartCoroutine(TournamentAnimations());
        }

        private void HandlePlayerListUpdated (PlayerInfo[] receivedPlayers)
        {
            myId = NetworkManager.Instance.MyPlayerInfo.Id;
            if (receivedPlayers.Length == 2 && receivedPlayers[0].Id == myId)
            {
                PlayerInfo myInfo = receivedPlayers[0];
                receivedPlayers[0] = receivedPlayers[1];
                receivedPlayers[1] = myInfo;
            }
            for (int i = 0; i < receivedPlayers.Length; i++)
            {
                if (playerSlots.TryGetValue(receivedPlayers[i].Id, out PlayerInfoSlot slot))
                {
                    slot.HandlePlayerInfo(receivedPlayers[i]);
                }
                else
                {
                    slot = CreateSlot(receivedPlayers[i]);
                    playerSlots.Add(receivedPlayers[i].Id, slot);
                    slot.transform.localScale = 0.01f * Vector3.one;
                }
            }
        }

        private void HandleRoundReceived (RoundInfo roundInfo)
        {
            this.roundInfo = roundInfo;
        }

        public void HandleTurnResultReceived (RoundInfo roundInfo)
        {
            GameManager.UpdatePlayers();
            foreach (var item in roundInfo.Matches)
            {
                playerSlots[item.Player1].HandlePlayerInfo(GameManager.GetPlayer(item.Player1));
                if (!string.IsNullOrEmpty(item.Player2))
                    playerSlots[item.Player2].HandlePlayerInfo(GameManager.GetPlayer(item.Player2));
            }
        }

        public void HandleStateChanged (string state)
        {
            
        }

        private void UseRoundInfo ()
        {
            int index = 0;
            GameManager.UpdatePlayers();
            foreach (var item in roundInfo.Matches)
            {
                if (item.Player1 == myId)
                {
                    myMatchIndex = index;
                    myMatch = item;
                    currentOpponentId = item.Player2;
                }
                else if (item.Player2 == myId)
                {
                    myMatchIndex = index;
                    myMatch = item;
                    currentOpponentId = item.Player1;
                }
                index++;
            }
        }

        private void UpdatePlayersInfoBits ()
        {
            foreach (var item in roundInfo.Matches)
                UpdateMatchInfo(item);
        }

        public void UpdateMatchInfo (MatchInfo matchInfo)
        {
            PlayerInfo p1, p2;
            p1 = GameManager.GetPlayer(matchInfo.Player1);
            playerSlots[matchInfo.Player1].HandlePlayerInfo(p1);
            if (matchInfo.Player2 != null)
            {
                p2 = GameManager.GetPlayer(matchInfo.Player2);
                playerSlots[matchInfo.Player2].HandlePlayerInfo(p2);
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

        private IEnumerator TournamentAnimations ()
        {
            yield return PlayersEntryAnimation();
            while (!isTournamentOver)
            {
                yield return RoundStartAnimation();
                yield return MatchStartAnimation();
                while (!roundInfo.IsOver)
                    yield return null;
                yield return MatchesEndedAnimation();
            }
        }

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
        }

        private IEnumerator RoundStartAnimation ()
        {
            // Wait for tournament info
            while (roundInfo == null)
                yield return null;
            UseRoundInfo();

            // Build tournament structure
            float moveToTournamentTime = settings.MoveToBubblesTime;
            for (int i = 0; i < matchesPositions.Length; i++)
                matchesPositions[i].gameObject.SetActive(i < roundInfo.Matches.Length);

            // TODO Round Number animation

            if (roundInfo.Matches.Length > 1)
            {
                // Move each player slot to its position with a jump
                int index = 0;
                foreach (var item in roundInfo.Matches)
                {
                    playerSlots[item.Player1].transform.MoveAndScaleTo(tournamentPositions[index++], moveToTournamentTime, true);
                    int p2Index = index++;
                    if (!string.IsNullOrEmpty(item.Player2))
                        playerSlots[item.Player2].transform.MoveAndScaleTo(tournamentPositions[p2Index], moveToTournamentTime, true);
                }
                //  Move tournament structure to view
                tournamentStructure.DOLocalMove(Vector3.zero, moveToTournamentTime / 2f);
                yield return new WaitForSeconds(moveToTournamentTime + settings.TournamentShowTime);
                // Hide tournament structure
                tournamentStructure.DOLocalMove(tournamentHiddenPosition, moveToTournamentTime / 2f);
            }
        }

        private IEnumerator MatchStartAnimation ()
        {
            bool haveOpponent = !string.IsNullOrEmpty(currentOpponentId);
            if (haveOpponent)
            {
                float moveToVersusTime = settings.MoveToVersusTime;
                int sidePositionsIndex = 0;
                for (int i = 0; i < matchBubbles.Length; i++)
                {
                    if (i == myMatchIndex)
                    {
                        // Move my opponent and mine slots to the center of the screen BIG
                        matchBubbles[i].gameObject.SetActive(false);
                        playerSlots[myId].transform.MoveAndScaleTo(versusPositions[1], moveToVersusTime);
                        playerSlots[currentOpponentId].transform.MoveAndScaleTo(versusPositions[0], moveToVersusTime);
                    }
                    else if (i < roundInfo.Matches.Length)
                    {
                        //      also Move each other player to the sides of the screen (small) according to their matches
                        matchBubbles[i].MoveAndScaleTo(sidePositions[sidePositionsIndex++], moveToVersusTime);
                    }
                }

                // TODO Play a versus animation

                float versusAnimationTime = settings.VersusAnimationTime;

                yield return new WaitForSeconds(moveToVersusTime + versusAnimationTime);
            }
            UpdatePlayersInfoBits();
            float dockingTime = settings.DockingTime;
            // Dock my opponent slot at the top of the screen and mine at the bottom
            if (haveOpponent)
                playerSlots[currentOpponentId].transform.MoveAndScaleTo(battlePositions[0], dockingTime);
            playerSlots[myId].transform.MoveAndScaleTo(battlePositions[1], dockingTime);
            if (!haveOpponent)
            {
                // If I am bye, move opponents' battles to the center of the screen
                for (int i = 0; i < matchBubbles.Length; i++)
                {
                    if (i == myMatchIndex)
                        continue;
                    if (i < roundInfo.Matches.Length)
                        matchBubbles[i].MoveAndScaleTo(centerPositions[i], dockingTime);
                }
            }
            else
            {
                // Create and move playmats to their position
                float movePlaymatsTime = settings.MovePlaymatsTime;
                if (playmats[0] == null)
                    playmats[0] = CreatePlaymat(0);
                playmats[0].MoveAndScaleTo(playmatPositions[0], movePlaymatsTime);
                if (playmats[1] == null)
                    playmats[1] = CreatePlaymat(1);
                playmats[1].MoveAndScaleTo(playmatPositions[1], movePlaymatsTime);
                yield return new WaitForSeconds(dockingTime);
            }
            OnTournamentIntroFinished?.Invoke();
        }

        private IEnumerator MatchesEndedAnimation ()
        {
            // TODO Reencapsulate the player and his opponent slot match

            // TODO Reparent matches bubbles to their structure

            // Fly playmats out
            float movePlaymatsTime = settings.MovePlaymatsTime;
            playmats[0].DOLocalMoveY(-700, movePlaymatsTime);
            playmats[1].DOLocalMoveY(700, movePlaymatsTime);

            // TODO     and present them in the center with record (1-0, 0-1, etc)

            // TODO Go to Tournament round start again

            roundInfo = null;
            yield return null;
        }

        #endregion

        private PlayerInfoSlot CreateSlot (PlayerInfo info)
        {
            PlayerInfoSlot newSlot = Instantiate(settings.GameInfoSlotPrefab, playerSlotsListParent.transform);
            newSlot.HandlePlayerInfo(info);
            return newSlot;
        }

        private Transform CreatePlaymat (int index)
        {
            Transform result = Instantiate(index == 0 ? settings.PlaymatTopPrefab : settings.PlaymatBottomPrefab, playmatPositions[index]).transform;
            result.localPosition = 700 * (index == 0 ? 1 : -1) * Vector3.up;
            return result;
        }
    }
}
