using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Tournament
{
    [CreateAssetMenu(menuName = "Fire Tournament/Settings", fileName = "TournamentGameSettings")]
    public class TournamentGameSettings : SingletonScriptableObject<TournamentGameSettings>
    {
        [Header("Game Configuration")]
        public int MaxPlayers;
        public float DelayBeforeStarting = 3f;
        [Header("Prefabs")]
        public PlayerInfoSlot RoomInfoSlotPrefab;
        public PlayerInfoSlot GameInfoSlotPrefab;
        public GameObject PlaymatTopPrefab;
        public GameObject PlaymatBottomPrefab;
        [Header("Animation Time")]
        public float IntroAnimationTime = 2f;
        public float DelayBetweenSlotsAppearing = 0.1f;
        public float MoveToBubblesTime = 1f;
        public float TournamentShowTime = 2f;
        public float MoveToVersusTime = 1f;
        public float VersusAnimationTime = 2f;
        public float DockingTime = 1f;
        public float MovePlaymatsTime = 0.7f;
    }
}
