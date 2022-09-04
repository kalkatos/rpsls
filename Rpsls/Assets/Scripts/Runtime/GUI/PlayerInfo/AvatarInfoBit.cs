using UnityEngine;
using UnityEngine.UI;
using Kalkatos.Network;

namespace Kalkatos.Rpsls
{
    public class AvatarInfoBit : PlayerInfoBit
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Sprite[] possibleSprites;

        public override void HandlePlayerInfo (PlayerInfo info)
        {
            avatarImage.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
        }
    }
}
