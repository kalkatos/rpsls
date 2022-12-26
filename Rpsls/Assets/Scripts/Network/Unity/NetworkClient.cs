using System;
using System.IO;
using Kalkatos.Network.Specific;
using Kalkatos.Network.Model;
using UnityEngine;
using Newtonsoft.Json;
using Kalkatos.FunctionsGame.Models;

namespace Kalkatos.Network.Unity
{
	public class NetworkClient : MonoBehaviour
	{
		public static NetworkClient Instance;

		private INetworkClient _networkClient = new AzureFunctionsNetworkClient();

		private void Awake ()
		{
			Instance = this;
		}

		/// <summary>
		/// Invokes the connect method on the Network interface.
		/// </summary>
		/// <param name="onSuccess"> True if it's new user </param>
		/// <param name="onFailure"> NetworkError with info on what happened. </param>
		public static void Connect (Action<bool> onSuccess, Action<NetworkError> onFailure)
		{
			string deviceId = SystemInfo.deviceUniqueIdentifier;
			// Invoke network
			Instance._networkClient.Connect(deviceId,
				(success) =>
				{
					LoginResponse response = (LoginResponse)success;
					onSuccess.Invoke(response.IsNewUser);
				},
				(failure) =>
				{
					// Player is not connected to the internet
					onFailure.Invoke((NetworkError)failure);
				});
		}
	}
}