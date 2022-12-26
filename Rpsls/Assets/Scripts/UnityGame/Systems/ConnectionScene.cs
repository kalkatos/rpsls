using System;
using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Systems
{
	public class ConnectionScene : MonoBehaviour
    {
		public static event Action OnNotConnectedError;

		private void Start ()
        {
			TryConnectionAsync();
        }

        private void TryConnectionAsync ()
        {
			Logger.Log("Trying connection...");
			NetworkClient.Connect(
				(success) =>
				{
					SceneManager.EndScene("Connection", "ToMenu");
					Storage.Save("IsNewUser", success ? 1 : 0);
					Logger.Log("Connected Successfully!");
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						OnNotConnectedError?.Invoke();
					Logger.Log("Connection Error: " + failure.Message);
				});
		}
    }
}
