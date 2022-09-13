using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{

    [CreateAssetMenu(menuName = "Rpsls/Game Settings", fileName = "RpslsGameSettings")]
    public class RpslsGameSettings : SingletonScriptableObject<RpslsGameSettings>
    {
        [Header("Game Configuration")]
        public int MaxPlayers;
        public float DelayBeforeStarting;
        [Header("Prefabs")]
        public PlayerInfoSlot RoomInfoSlotPrefab;
        public PlayerInfoSlot GameInfoSlotPrefab;
        public GameObject PlaymatTopPrefab;
        public GameObject PlaymatBottomPrefab;
        [Header("Animation Time")]
        public float IntroAnimationTime = 2f;
        public float DelayBetweenSlotsAppearing = 0.1f;
        public float MoveToTournamentTime = 1f;
        public float TournamentShowTime = 2f;
        public float MoveToVersusTime = 1f;
        public float VersusAnimationTime = 2f;
        public float DockingTime = 1f;
    }
}
