// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if KALKATOS_NETWORK

using UnityEngine;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Scriptable
{
	public class GoToConnectionScene : MonoBehaviour
    {
        [SerializeField] private ScreenSignal connectionScene;

        private void Start ()
        {
            if (!NetworkClient.IsConnected)
                connectionScene?.Emit();
		}
    }
}

#endif