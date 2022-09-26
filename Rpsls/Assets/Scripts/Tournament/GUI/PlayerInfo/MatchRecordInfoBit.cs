using UnityEngine;
using Kalkatos.Network;
using TMPro;

namespace Kalkatos.Tournament
{
    public class MatchRecordInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text matchRecordText;
        [SerializeField] private GameObject victoryCounterObj;
        [SerializeField] private Animator counter1;
        [SerializeField] private Animator counter2;
        [SerializeField] private Animator counter3;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            bool hasState = info.CustomData.TryGetValue(Keys.ClientStateKey, out object stateObj);
            string state = stateObj != null ? stateObj.ToString() : "";
            if ((info.CustomData.TryGetValue(Keys.IsByeKey, out object isBye) && bool.Parse(isBye.ToString()))
                || !hasState 
                || state == ClientState.InGame
                || state == ClientState.Undefined
                || state == ClientState.BetweenMatches
                || state == ClientState.GameOver)
            {
                //matchRecordText.gameObject.SetActive(false);
                victoryCounterObj.SetActive(false);
                counter1.SetTrigger("0");
                counter2.SetTrigger("0");
                counter3.SetTrigger("0");
                return;
            }
            
            if (info.CustomData.TryGetValue(Keys.MatchRecordKey, out object record))
            {
                string[] split = record.ToString().Split('-');
                int myVictories = int.Parse(split[0]);
                victoryCounterObj.SetActive(true);
                counter1.SetTrigger((myVictories == 0 ? 0 : 1 + myVictories / 3).ToString());
                counter2.SetTrigger((myVictories < 2 ? 0 : 1 + (myVictories % 3 < 2 ? 0 : myVictories / 3)).ToString()); // Lógica doida
                counter3.SetTrigger((myVictories < 3 ? 0 : 1 + (myVictories % 3 < 3 ? 0 : myVictories / 3)).ToString());
                //matchRecordText.gameObject.SetActive(true);
                //matchRecordText.text = record.ToString();
            }
        }
    }
}
