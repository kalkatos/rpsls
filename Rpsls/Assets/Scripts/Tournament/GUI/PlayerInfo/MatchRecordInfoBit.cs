using UnityEngine;
using Kalkatos.Network;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kalkatos.Tournament
{
    public class MatchRecordInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text matchRecordText;
        [SerializeField] private GameObject victoryCounterObj;
        [SerializeField] private Animator counter1;
        [SerializeField] private Animator counter2;
        [SerializeField] private Animator counter3;

        private Dictionary<Animator, string> currentStates = new Dictionary<Animator, string>();

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            bool hasState = info.CustomData.ContainsKey(Keys.ClientStateKey);
            bool isBye = info.CustomData.TryGetValue(Keys.IsByeKey, out object isByeObj) && bool.Parse(isByeObj.ToString());
            
            if (isBye
                || !hasState 
                || state == "MatchOff")
            {
                SetTriggerState(counter1, "0");
                SetTriggerState(counter2, "0");
                SetTriggerState(counter3, "0");
                victoryCounterObj.SetActive(false);
                return;
            }
            else if (state == "MatchOn"
                && info.CustomData.TryGetValue(Keys.MatchRecordKey, out object record))
            {
                if (!victoryCounterObj.activeSelf)
                    victoryCounterObj.SetActive(true);
                string[] split = record.ToString().Split('-');
                int myVictories = int.Parse(split[0]);
                string counter1Trigger = (myVictories / 3 + (myVictories % 3 > 0 ? 1 : 0)).ToString();
                string counter2Trigger = (myVictories / 3 + (myVictories % 3 > 1 ? 1 : 0)).ToString();
                string counter3Trigger = (myVictories / 3).ToString();
                SetTriggerState(counter1, counter1Trigger);
                SetTriggerState(counter2, counter2Trigger);
                SetTriggerState(counter3, counter3Trigger);
            }
        }

        private void SetTriggerState (Animator counter, string trigger)
        {
            if (!currentStates.ContainsKey(counter))
                currentStates.Add(counter, trigger);
            else if (currentStates[counter] == trigger)
                return;
            currentStates[counter] = trigger;
            counter.SetTrigger(trigger);
        }
    }
}
