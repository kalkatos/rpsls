using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class BoardInfoBit : PlayerInfoBit
    {
        [SerializeField] private GameObject myBoard;
        [SerializeField] private GameObject othersBoard;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            myBoard.SetActive(info.IsMe);
            othersBoard.SetActive(!info.IsMe);
        }
    }
}
