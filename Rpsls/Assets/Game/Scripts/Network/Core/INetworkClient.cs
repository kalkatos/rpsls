// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using System;
using Kalkatos.Network.Model;

namespace Kalkatos.Network
{
	/// <summary>
	/// Base interface for sending and receiving data from a server.
	/// </summary>
	public interface INetworkClient
	{
		bool IsConnected { get; }
		bool IsInRoom { get; }
		PlayerInfo[] Players { get; }
		PlayerInfo MyInfo { get; }
		MatchInfo MatchInfo { get; }
		StateInfo StateInfo { get; }

		void Connect (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void FindMatch (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void LeaveMatch (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void GetMatch (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void SetNickname (string nickname);
		void SetPlayerData (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void SendAction (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void GetMatchState (object parameter, Action<object> onSuccess, Action<object> onFailure);
		void Get (byte key, object parameter, Action<object> onSuccess, Action<object> onFailure);
		void Post (byte key, object parameter, Action<object> onSuccess, Action<object> onFailure);
	}
}