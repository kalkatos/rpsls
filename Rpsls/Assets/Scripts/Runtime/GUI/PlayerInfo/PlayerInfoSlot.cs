using System.Collections.Generic;
using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class PlayerInfoSlot : PlayerInfoBit
    {
        [SerializeField] private List<PlayerInfoBit> additionalInfo;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            for (int i = 0; i < additionalInfo.Count; i++)
                additionalInfo[i].HandlePlayerInfo(info);
        }
    }

    public abstract class PlayerInfoBit : MonoBehaviour
    {
        public abstract void HandlePlayerInfo (PlayerInfo info);
    }
}
