using UnityEngine;
using UnityEngine.UI;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class AvatarInfoBit : PlayerInfoBit
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Sprite[] possibleSprites;

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            avatarImage.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
        }
    }
}
