using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kalkatos.Rpsls
{
    public class PlayerInfoSlot : MonoBehaviour
    {
        [SerializeField] private GameObject[] statusObjects;
        [SerializeField] private TMP_Text nicknameText;

        public void SetStatus (RoomStatus status)
        {
            for (int i = 0; i < statusObjects.Length; i++)
                statusObjects[i].SetActive(i == (int)status);
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
