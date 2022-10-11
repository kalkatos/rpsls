using System;
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
        public float TurnDuration = 5f;
        [Header("Tournament Options")]
        public int TurnVictories;
        public PlayerAmountDef[] NumberOfRoundsDefinition;
        public bool doPlayoffs;
        public PlayerAmountDef[] NumberOfPlayersForPlayoffs;
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
        public float MoveToRankingTime = 0.7f;

        public int GetNumberOfRounds (int numberOfPlayers)
        {
            if (NumberOfRoundsDefinition == null || NumberOfRoundsDefinition.Length == 0)
                return 1;
            for (int i = 0; i < NumberOfRoundsDefinition.Length - 1; i += 2)
                if (numberOfPlayers <= NumberOfRoundsDefinition[i].Players)
                    return NumberOfRoundsDefinition[i].Amount;
            return NumberOfRoundsDefinition[NumberOfRoundsDefinition.Length - 1].Amount;
        }
    }

    [Serializable]
    public class PlayerAmountDef
    {
        public int Players;
        public int Amount;
    }
}
