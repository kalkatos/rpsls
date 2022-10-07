using UnityEngine;
using Kalkatos.Network;

namespace Kalkatos.Tournament
{
    public class StatusInfoBit : PlayerInfoBit
    {
        [SerializeField] private GameObject[] statusObjects;

        private void Awake ()
        {
            for (int i = 0; i < statusObjects.Length; i++)
                statusObjects[i].SetActive(false);
        }

        private void SetStatus (int status)
        {
            for (int i = 0; i < statusObjects.Length; i++)
                statusObjects[i].SetActive(i == status);
        }

        public override void HandlePlayerInfo (PlayerInfo info, string state)
        {
            int status = (int)RoomStatus.Idle;
            if (info.CustomData.ContainsKey(RoomManager.RoomStatusKey))
                status = int.Parse(info.CustomData[RoomManager.RoomStatusKey].ToString());
            SetStatus(status);
        }
    }
}
