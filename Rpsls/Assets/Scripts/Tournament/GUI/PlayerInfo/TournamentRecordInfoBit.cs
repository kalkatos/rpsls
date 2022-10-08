using UnityEngine;
using Kalkatos.Network;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kalkatos.Tournament
{
    public class TournamentRecordInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text tournamentRecordText;
        [SerializeField] private GameObject recordObj;

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            bool active = state.Contains("TRecOn");
            recordObj.SetActive(active);
            if (active && info.CustomData.TryGetValue(Keys.TournamentRecordKey, out object record))
                tournamentRecordText.text = record.ToString();
        }
    }
}
