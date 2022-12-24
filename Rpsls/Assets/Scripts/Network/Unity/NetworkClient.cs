using System;
using System.IO;
using Kalkatos.Network.Specific;
using Kalkatos.Network.Model;
using UnityEngine;
using Newtonsoft.Json;

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

		public static void Connect (Action<object> onSuccess, Action<object> onFailure)
		{
			// Find and load player info file
			PlayerInfo playerInfo = null;
			try
			{
				string infoFilePath = Application.persistentDataPath + "/player-info.json";
				if (File.Exists(infoFilePath))
				{
					string content = File.ReadAllText(infoFilePath);
					playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(content);
				}
			}
			catch
			{
				Debug.Log("Could not find player info.");
			}
			// Invoke network
			Instance._networkClient.Connect(playerInfo?.PlayerId,
				(success) =>
				{
					// Player were found on network OR it's new player logging in with device id
					// If player found, ask for credentials
					// If new player return null
					// Save player info on file
					onSuccess.Invoke(success);
				},
				(failure) =>
				{
					// Player not found OR not connected
					if (failure is NetworkError)
					{
						if (((NetworkError)failure).Tag == NetworkErrorTag.NotFound)
						{
							if (playerInfo != null)
							{
								Debug.LogError("Fatal error: player has local file, but server does not.");
								// TODO Send a log for fatal error player not found
							}
						}
						onFailure.Invoke(failure);
					}
				});
		}
	}
}