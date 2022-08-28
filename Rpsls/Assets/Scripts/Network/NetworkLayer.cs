using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class NetworkLayer : MonoBehaviour
    {
        private RpslsGameSettings settings;

        private void Awake ()
        {
            settings = RpslsGameSettings.Instance;
        }

        private void Start ()
        {
            NetworkManager.Instance.Connect();
        }
    }
}
