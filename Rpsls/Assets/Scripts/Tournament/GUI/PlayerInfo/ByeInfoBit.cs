using Kalkatos.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Tournament
{
    public class ByeInfoBit : PlayerInfoBit
    {
        [SerializeField] private GameObject byeBadge;

        private void Awake ()
        {
            byeBadge.SetActive(false);
        }

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            if (state == "ByeOn")
            {
                if (info.CustomData.ContainsKey(Keys.IsByeKey))
                    byeBadge.SetActive(bool.Parse(info.CustomData[Keys.IsByeKey].ToString()));
            }
            else if (state == "ByeOff")
                byeBadge.SetActive(false);
        }
    }
}
