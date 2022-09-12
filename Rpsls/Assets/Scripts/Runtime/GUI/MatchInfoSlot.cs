using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kalkatos.Rpsls
{
    public class MatchInfoSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text matchNameText;

        public void SetIndex (int index)
        {
            matchNameText.text = "Match " + (index + 1);
        }
    }
}
