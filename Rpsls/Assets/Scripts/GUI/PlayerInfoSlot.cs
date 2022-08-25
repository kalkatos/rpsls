using UnityEngine;
using TMPro;

namespace Kalkatos.Rpsls
{
    public class PlayerInfoSlot : MonoBehaviour
    {
        [SerializeField] private GameObject[] statusObjects;
        [SerializeField] private TMP_Text nicknameText;

        private RoomStatus currentStatus = RoomStatus.Idle;

        public RoomStatus Status => currentStatus;

        private void Awake ()
        {
            SetStatus(currentStatus, true);
        }

        public void SetStatus (RoomStatus status, bool force = false)
        {
            if (force || status != currentStatus)
            {
                currentStatus = status;
                for (int i = 0; i < statusObjects.Length; i++)
                    statusObjects[i].SetActive(i == (int)status);
            }
        }

        public void SetNickname (string nickname)
        {
            nicknameText.text = nickname;
        }
    }

    public enum RoomStatus
    {
        Idle = 0,
        Ready = 1,
        Master = 2
    }
}
