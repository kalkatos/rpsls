using Kalkatos.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public class ConnectionManager : MonoBehaviour
    {
        private void Awake ()
        {
            NetworkManager.OnLoginSuccess += HandleLoginSuccess;
        }

        private void OnDestroy ()
        {
            NetworkManager.OnLoginSuccess -= HandleLoginSuccess;
        }

        private void Start ()
        {
            NetworkManager.Instance.Connect();
        }

        private void HandleLoginSuccess (object obj)
        {
            SceneManager.EndScene("Connection");
        }
    }
}
