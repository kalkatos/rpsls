using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    [CreateAssetMenu(menuName = "Rpsls/Settings", fileName = "RpslsGameSettings")]
    public class Settings : ScriptableObject
    {
        [Header("Game Configuration")]
        public int MaxPlayers;
        [Header("Prefabs")]
        public PlayerInfoSlot PlayerInfoSlotPrefab;
    }
}
