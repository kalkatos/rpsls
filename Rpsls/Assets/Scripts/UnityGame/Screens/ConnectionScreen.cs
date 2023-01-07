using System;
using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Screens
{
	public class ConnectionScreen : MonoBehaviour
    {
		[SerializeField] private UnityEvent onNotConnectedError;

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
					ScreenManager.GoToNextScene();
					Storage.Save("IsNewUser", success ? 1 : 0);
					Logger.Log("Connected Successfully!");
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						onNotConnectedError.Invoke();
					Logger.Log("Connection Error: " + failure.Message);
				});
		}
    }
}
