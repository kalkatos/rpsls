// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kalkatos.Network.Model;
using Newtonsoft.Json;

namespace Kalkatos.Network
{
	/// <summary>
	/// Network Client implementation using Azure Functions.
	/// </summary>
	public class AzureFunctionsNetworkClient : NetworkEventDispatcher, INetworkClient
	{
		private Dictionary<string, string> uris = new Dictionary<string, string>
		{
			{ "SetPlayerData", "SetPlayerData" },
			{ "LogIn", "LogIn" },
			{ "FindMatch", "FindMatch" },
			{ "GetMatch", "GetMatch" },
			{ "LeaveMatch", "LeaveMatch" },
			{ "SendAction", "SendAction" },
			{ "GetMatchState", "GetMatchState" },
			{ "GetGameSettings", "GetGameSettings" }
		};
		private string functionsPrefix = "https://myapp123.azurewebsites.net/api/";
		private string localFunctionsPrefix = "http://localhost:7089/api/";
		private ICommunicator communicator;
		private bool mustRunLocally = false;

		public AzureFunctionsNetworkClient (ICommunicator communicator)
		{
			this.communicator = communicator;
		}

		public string MyId { get; private set; }
		public bool IsConnected { get; private set; }
		public bool IsInRoom { get; private set; }
		public PlayerInfo[] Players { get; private set; }
		public PlayerInfo MyInfo { get; private set; }
		public MatchInfo MatchInfo { get; private set; }
		public StateInfo StateInfo { get; private set; }

		// ████████████████████████████████████████████ P U B L I C ████████████████████████████████████████████

		/// <summary>
		/// 
		/// </summary>
		/// <param screenName="parameter">A <typeparamref screenName="LoginRequest"/></param>
		/// <param screenName="onSuccess">A <typeparamref screenName="LoginResponse"/> with matchResponse on the connection.</param>
		/// <param screenName="onFailure">A <typeparamref screenName="NetworkError"/> with the reason it did not connect.</param>
		public void Connect (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (parameter == null)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is null, it must be an identifier string to connect." });
				return;
			}

			if (!(parameter is LoginRequest))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is not of the expected type." });
				return;
			}
			if (mustRunLocally)
				functionsPrefix = localFunctionsPrefix;
			else
				functionsPrefix = Storage.Load("UrlPrefix", functionsPrefix);

			_ = ConnectAsync((LoginRequest)parameter, onSuccess, onFailure);
		}

		public void SetNickname (string nickname)
		{
			if (!IsConnected)
			{
				Logger.LogError("Not connected.");
				return;
			}
			SetPlayerData(new Dictionary<string, string> { { "Nickname", nickname } }, null, null);
		}

		public void SetPlayerData (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (!IsConnected)
			{
				Logger.LogError("Not connected.");
				return;
			}

			if (!(parameter is Dictionary<string, string>))
			{
				Logger.LogError("Wrong parameter. Must be a Dictionary<string, string>.");
				return;
			}

			var changedData = (Dictionary<string, string>)parameter;
			SetPlayerDataRequest request = new SetPlayerDataRequest
			{
				PlayerId = MyId,
				Data = changedData
			};
			
			_ = SetPlayerDataAsync(request, onSuccess, onFailure);
		}

		public void FindMatch (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (!IsConnected)
			{
				Logger.LogError("Not connected.");
				return;
			}

			if (!(parameter is FindMatchRequest))
			{
				Logger.LogError("Wrong parameter. Must be a Dictionary<string, string>.");
				return;
			}

			_ = FindMatchAsync((FindMatchRequest)parameter, onSuccess, onFailure);
		}

		public void GetMatch (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (!IsConnected)
			{
				Logger.LogError("Not connected.");
				return;
			}

			if (!(parameter is MatchRequest))
			{
				Logger.LogError("Wrong parameter. Must be a MatchRequest.");
				return;
			}

			_ = GetMatchAsync((MatchRequest)parameter, onSuccess, onFailure);
		}

		public void LeaveMatch (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (!IsConnected)
			{
				Logger.LogError("Not connected.");
				return;
			}

			if (!(parameter is MatchRequest))
			{
				Logger.LogError("Wrong parameter. Must be a MatchRequest.");
				return;
			}

			_ = LeaveMatchAsync((MatchRequest)parameter, onSuccess, onFailure);
		}

		public void SendAction (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (parameter == null)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is null, it must be an identifier string to connect." });
				return;
			}

			if (!(parameter is ActionRequest))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is not of the expected type." });
				return;
			}

			_ = SendActionAsync(JsonConvert.SerializeObject(parameter), onSuccess, onFailure);
		}

		public void GetMatchState (object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			if (parameter == null)
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is null, it must be an identifier string to connect." });
				return;
			}

			if (!(parameter is StateRequest))
			{
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.WrongParameters, Message = "Parameter is not of the expected type." });
				return;
			}

			_ = GetMatchStateAsync(JsonConvert.SerializeObject(parameter), onSuccess, onFailure);
		}

		public void Get (byte key, object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			
		}

		public void Post (byte key, object parameter, Action<object> onSuccess, Action<object> onFailure)
		{
			
		}

		/// ████████████████████████████████████████████ P R I V A T E ████████████████████████████████████████████

		private async Task<string> Post (string uriTag, string content)
		{
			TaskCompletionSource<string> taskAwaiter = new TaskCompletionSource<string>();
			communicator.Post(functionsPrefix + uris[uriTag], content, response => taskAwaiter.TrySetResult(response));
			return await taskAwaiter.Task;
		}

		private async Task ConnectAsync (LoginRequest connectInfo, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("LogIn", JsonConvert.SerializeObject(connectInfo));
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Result received: {result}");
				LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
				if (loginResponse == null || loginResponse.IsError)
				{
					string message = loginResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(Connect)}: {message}");
					onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotAvailable, Message = message });
				}
				else
				{
					string configResult = await Post("GetGameSettings", JsonConvert.SerializeObject(new GameDataRequest
					{
						GameId = connectInfo.GameId,
						PlayerId = loginResponse.PlayerId
					}));
					GameDataResponse gameDataResponse = JsonConvert.DeserializeObject<GameDataResponse>(configResult);
					if (gameDataResponse?.Settings != null && !mustRunLocally)
					{
						foreach (var item in gameDataResponse.Settings)
							if (uris.ContainsKey(item.Key))
								uris[item.Key] = gameDataResponse.Settings[item.Key];
					}
					else
						Logger.Log("Error getting game settings from server.");

					IsConnected = true;
					MyId = loginResponse.PlayerId;
					MyInfo = loginResponse.MyInfo;
					onSuccess?.Invoke(loginResponse);
					RaiseEvent((byte)NetworkEventKey.Connect, loginResponse);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(Connect)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotAvailable, Message = "Server internal error." });
			}
		}

		private async Task SetPlayerDataAsync (SetPlayerDataRequest request, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("SetPlayerData", JsonConvert.SerializeObject(request));
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Result received: {result}");
				PlayerInfoResponse playerInfoResponse = JsonConvert.DeserializeObject<PlayerInfoResponse>(result);
				if (playerInfoResponse == null || playerInfoResponse.IsError)
				{
					string message = playerInfoResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(SetPlayerData)}: {message}");
					onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotAvailable, Message = message });
				}
				else
				{
					MyInfo = playerInfoResponse.PlayerInfo;
					onSuccess?.Invoke(MyInfo);
					RaiseEvent((byte)NetworkEventKey.SetPlayerData, playerInfoResponse);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(SetPlayerDataAsync)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected to the internet." });
			}
		}

		private async Task FindMatchAsync (FindMatchRequest request, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("FindMatch", JsonConvert.SerializeObject(request));
				Response findMatchResponse = JsonConvert.DeserializeObject<Response>(result);
				if (findMatchResponse == null || findMatchResponse.IsError)
				{
					string message = findMatchResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(FindMatch)}: {message}");
					onFailure?.Invoke(new NetworkError { Message = message }); 
				}
				else
				{
					onSuccess?.Invoke(null);
					RaiseEvent((byte)NetworkEventKey.FindMatch, null);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(FindMatch)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotConnected, Message = "Not connected to the internet." });
			}
		}

		private async Task GetMatchAsync (MatchRequest request, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("GetMatch", JsonConvert.SerializeObject(request));
				MatchResponse matchResponse = JsonConvert.DeserializeObject<MatchResponse>(result);
				if (matchResponse == null || matchResponse.IsError)
				{
					string message = matchResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(GetMatch)}: {message}");
					MatchInfo = null;
					onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotFound, Message = message }); 
				}
				else
				{
					Logger.Log($"Got match = {JsonConvert.SerializeObject(matchResponse)}");
					if (matchResponse.IsOver)
					{
						MatchInfo = null;
						onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.NotFound, Message = "Match is over." });
						return;
					}
					MatchInfo = new MatchInfo { MatchId = matchResponse.MatchId, Players = matchResponse.Players };
					onSuccess?.Invoke(MatchInfo);
					RaiseEvent((byte)NetworkEventKey.GetMatch, MatchInfo);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(GetMatch)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.Undefined, Message = "Error getting match." });
			}
		}

		private async Task LeaveMatchAsync (MatchRequest request, Action<object> onSuccess, Action<object> onFailure)
		{
			MatchInfo = null;
			try
			{
				string result = await Post("LeaveMatch", JsonConvert.SerializeObject(request));
				Response leaveMatchResponse = JsonConvert.DeserializeObject<Response>(result);
				if (leaveMatchResponse == null || leaveMatchResponse.IsError)
				{
					string message = leaveMatchResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(LeaveMatch)}: {message}");
					onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.Undefined, Message = message });
				}
				else
				{
					string matchId = MatchInfo?.MatchId ?? "<unknown>";
					Logger.Log($"Left match {matchId}, Message = {leaveMatchResponse.Message}");
					onSuccess?.Invoke(leaveMatchResponse);
					RaiseEvent((byte)NetworkEventKey.LeaveMatch, leaveMatchResponse);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(LeaveMatch)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.Undefined, Message = $"Exception trying to leave match = {e.Message}" });
			}
		}

		private async Task SendActionAsync (string actionRequestSerialized, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("SendAction", actionRequestSerialized);
				ActionResponse actionResponse = JsonConvert.DeserializeObject<ActionResponse>(result);
				if (actionResponse == null || actionResponse.IsError)
				{
					string message = actionResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(SendAction)}: {message}");
					onFailure?.Invoke(new NetworkError { Message = message }); 
				}
				else
				{
					StateInfo = actionResponse.AlteredState;
					onSuccess?.Invoke(StateInfo);
					RaiseEvent((byte)NetworkEventKey.SendAction, StateInfo);
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(SendAction)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.Undefined, Message = "Error sending action." });
			}
		}

		private async Task GetMatchStateAsync (string stateRequestSerialized, Action<object> onSuccess, Action<object> onFailure)
		{
			try
			{
				string result = await Post("GetMatchState", stateRequestSerialized);
				StateResponse stateResponse = JsonConvert.DeserializeObject<StateResponse>(result);
				if (stateResponse == null || stateResponse.IsError)
				{
					string message = stateResponse?.Message ?? "Server internal error";
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(GetMatchState)}: {message}");
					onFailure?.Invoke(new NetworkError { Message = message }); 
				}
				else
				{
					StateInfo = stateResponse.StateInfo;
					onSuccess?.Invoke(StateInfo);
					RaiseEvent((byte)NetworkEventKey.GetMatchState, StateInfo);
					Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Got match state === {JsonConvert.SerializeObject(StateInfo)}");
				}
			}
			catch (Exception e)
			{
				Logger.Log($"[{nameof(AzureFunctionsNetworkClient)}] Error in {nameof(GetMatchState)}: {e}");
				onFailure?.Invoke(new NetworkError { Tag = NetworkErrorTag.Undefined, Message = "Error getting match state." });
			}
		}
	}
}