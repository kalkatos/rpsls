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

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            if (info.CustomData.ContainsKey(Keys.IsByeKey))
                byeBadge.SetActive(bool.Parse(info.CustomData[Keys.IsByeKey].ToString()));
        }
    }
}
