using UnityEngine;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Scriptable
{
	public class GoToConnectionScene : MonoBehaviour
    {
        [SerializeField] private ScreenSignal connectionScene;

        void Awake ()
        {
            if (!NetworkClient.IsConnected)
                connectionScene?.Emit();
		}
    }
}