// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if UNITY_2018_1_OR_NEWER

using System;
using System.Collections.Generic;
using Kalkatos.Network.Model;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kalkatos.Network.Unity
{
	/// <summary>
	/// Unity MonoBehaviour wrapper to invoke INetworkClient methods.
	/// </summary>
	public class NetworkClient : MonoBehaviour
	{
		[SerializeField] private string gameId;

		private static NetworkClient instance;
		private static INetworkClient networkClient;
		private static string playerId;
		private static string playerRegion = "Default";
		private static string nickname;
		private static string localTestToken;

		private const string consonantsUpper = "BCDFGHJKLMNPQRSTVWXZ";
		private const string consonantsLower = "bcdfghjklmnpqrstvwxz";
		private const string vowels = "aeiouy";

		private string nicknameKey = "Nickname";

		public static bool IsConnected => networkClient.IsConnected;
		public static PlayerInfo MyInfo => networkClient.MyInfo;
		public static MatchInfo MatchInfo => networkClient.MatchInfo;
		public static StateInfo StateInfo => networkClient.StateInfo;

		private void Awake ()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
			{
				Destroy(this);
				return;
			}

			networkClient = new AzureFunctionsNetworkClient(new UnityWebRequestComnunicator(this));
			//networkClient = new AzureFunctionsNetworkClient(new HttpClientCommunicator());
			DontDestroyOnLoad(this);
#if UNITY_EDITOR
			nicknameKey += $"-{GetLocalDebugToken()}";
#endif
			nickname = Storage.Load(nicknameKey, "");
			if (string.IsNullOrEmpty(nickname))
			{
				nickname = "Guest-" + RandomName(6);
				SaveNicknameLocally(nickname);
			}
		}

		// ████████████████████████████████████████████ P U B L I C ████████████████████████████████████████████

		public static void SetNickname (string nick)
		{
			SaveNicknameLocally(nick);
			SendNicknameToServer(nick);
		}

		public static void SetPlayerData (Dictionary<string, string> data, Action<PlayerInfo> onSuccess, Action<Response> onFailure)
		{

			networkClient.SetPlayerData(data, (success) => onSuccess?.Invoke((PlayerInfo)success), (failure) => onFailure?.Invoke((Response)failure));
		}

		/// <summary>
		/// Invokes the connect method on the Network interface.
		/// </summary>
		/// <param screenName="onSuccess"> True if it's new user </param>
		/// <param screenName="onFailure"> <typeparamref screenName="NetworkError"/> with info on what happened. </param>
		public static void Connect (Action<bool> onSuccess, Action<NetworkError> onFailure)
		{
			string deviceId = GetDeviceIdentifier();
			Logger.Log("Connecting with identifier " + deviceId);

			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}

			// Invoke network
			networkClient.Connect(new LoginRequest { Identifier = deviceId, GameId = instance.gameId, Nickname = nickname, Region = playerRegion },
				(success) =>
				{
					LoginResponse response = (LoginResponse)success;
					playerId = response.PlayerId;
					SaveNicknameLocally(response.MyInfo.Nickname);
					onSuccess?.Invoke(response.IsAuthenticated);
				},
				(failure) =>
				{
					// Player is not connected to the internet
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		/// <summary>
		/// Tries to find a match.
		/// </summary>
		/// <param screenName="onSuccess"> String with the matchmaking ticket. </param>
		/// <param screenName="onFailure"> <typeparamref screenName="NetworkError"/> with info on what happened. </param>
		public static void FindMatch (Action<string> onSuccess, Action<NetworkError> onFailure)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}

			if (string.IsNullOrEmpty(playerId))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected. Connect first." });
				return;
			}
			networkClient.FindMatch(
				new FindMatchRequest
				{
					GameId = instance.gameId,
					PlayerId = playerId,
					Region = playerRegion
				},
				(success) =>
				{
					onSuccess?.Invoke("Success");
				},
				(failure) =>
				{
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		public static void GetMatch (Action<MatchInfo> onSuccess, Action<NetworkError> onFailure)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}

			if (string.IsNullOrEmpty(playerId))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected. Connect first." });
				return;
			}

			networkClient.GetMatch(
				new MatchRequest
				{
					PlayerId = playerId,
					MatchId = networkClient.MatchInfo?.MatchId,
					GameId = instance.gameId,
					Region = playerRegion
				},
				(success) =>
				{
					MatchInfo matchInfo = (MatchInfo)success;
					onSuccess?.Invoke(matchInfo);
				},
				(failure) =>
				{
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		public static void LeaveMatch (Action<string> onSuccess, Action<NetworkError> onFailure)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}

			if (string.IsNullOrEmpty(playerId))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected. Connect first." });
				return;
			}
			networkClient.LeaveMatch(
				new MatchRequest 
				{ 
					GameId = instance.gameId, 
					Region = playerRegion,
					PlayerId = playerId,
					MatchId = networkClient.MatchInfo?.MatchId ?? ""
				},
				(success) =>
				{
					onSuccess?.Invoke("Success Leaving Match");
				},
				(failure) =>
				{
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		public static void SendAction (ActionInfo action, Action<StateInfo> onSuccess, Action<NetworkError> onFailure)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}
			if (MatchInfo == null || string.IsNullOrEmpty(MatchInfo.MatchId))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotAvailable, Message = "Does not have a match to send action to." });
				return;
			}

			ActionRequest request = new ActionRequest
			{
				PlayerId = playerId,
				MatchId = MatchInfo.MatchId,
				Action = action
			};
			networkClient.SendAction(request,
				(success) =>
				{
					StateInfo state = (StateInfo)success;
					onSuccess?.Invoke(state);
				},
				(failure) =>
				{
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		public static void GetMatchState (Action<StateInfo> onSuccess, Action<NetworkError> onFailure)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected });
				return;
			}

			if (string.IsNullOrEmpty(playerId))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected. Connect first." });
				return;
			}

			int lastHash = StateInfo?.Hash ?? 0;
			StateRequest request = new StateRequest
			{
				PlayerId = playerId,
				MatchId = MatchInfo.MatchId,
				LastHash = lastHash
			};
			networkClient.GetMatchState(request,
				(success) =>
				{
					StateInfo state = (StateInfo)success;
					onSuccess?.Invoke(state);
				},
				(failure) =>
				{
					onFailure?.Invoke((NetworkError)failure);
				});
		}

		// ████████████████████████████████████████████ P R I V A T E ████████████████████████████████████████████

		private static string GetDeviceIdentifier ()
		{
			string deviceId = SystemInfo.deviceUniqueIdentifier;
			if (deviceId == SystemInfo.unsupportedIdentifier)
			{
				Logger.Log("Getting a local unique identifier");
				deviceId = Storage.Load("LocalUniqueIdentifier", Guid.NewGuid().ToString());
				Storage.Save("LocalUniqueIdentifier", deviceId);
			}
#if UNITY_EDITOR
			return $"{deviceId}-{GetLocalDebugToken()}";
#else
			return deviceId;
#endif
		}

		private static string GetLocalDebugToken ()
		{
			// Local test token
			localTestToken = "editor";
#if PARREL_SYNC && UNITY_EDITOR
			string cloneSuffix = ParrelSync.ClonesManager.GetArgument();
			if (!string.IsNullOrEmpty(cloneSuffix))
				localTestToken = cloneSuffix;
#endif
			return localTestToken;
		}

		private static string RandomName (int length)
		{
			string result = "";
			for (int i = 0; i < length; i++)
			{
				if (i == 0)
					result += consonantsUpper[Random.Range(0, consonantsUpper.Length)];
				else if (i % 2 == 0)
					result += consonantsLower[Random.Range(0, consonantsLower.Length)];
				else
					result += vowels[Random.Range(0, vowels.Length)];
			}
			return result;
		}

		private static void SaveNicknameLocally (string nick)
		{
			nickname = nick;
			Storage.Save(instance.nicknameKey, nick);
		}

		private static void SendNicknameToServer (string nick)
		{
			if (IsConnected)
				networkClient.SetNickname(nick);
		}
	}
}

#endif