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
        public PlayerInfoSlot PlayerInfoSlotPrefab;
        [Header("Session Info")]
        public string CurrentRoomName;
        public bool IAmMasterClient;
    }
}
