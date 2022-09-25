using UnityEngine;
using Kalkatos.Network;
using TMPro;

namespace Kalkatos.Tournament
{
    public class MatchRecordInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text matchRecordText;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            matchRecordText.gameObject.SetActive(false);
            if (info.CustomData.TryGetValue(Keys.IsByeKey, out object isBye) && bool.Parse(isBye.ToString()))
                return;

            if (info.CustomData.TryGetValue(Keys.PlayerStatusKey, out object status))
            {
                if (status.ToString() == ClientState.WaitingTurnResult)
                {
                    if (info.CustomData.TryGetValue(Keys.MatchRecordKey, out object record))
                    {
                        matchRecordText.gameObject.SetActive(true);
                        matchRecordText.text = record.ToString(); 
                    }
                }
            }
        }
    }
}
