using UnityEngine;
using TMPro;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class NickInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text nicknameText;

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            nicknameText.text = info.Nickname;
        }
    }
}
