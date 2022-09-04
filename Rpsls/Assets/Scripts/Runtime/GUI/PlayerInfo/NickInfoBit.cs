using UnityEngine;
using TMPro;
using Kalkatos.Network;

namespace Kalkatos.Rpsls
{
    public class NickInfoBit : PlayerInfoBit
    {
        [SerializeField] private TMP_Text nicknameText;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            nicknameText.text = info.Nickname + (info.IsMe ? " (me)" : "");
        }
    }
}
