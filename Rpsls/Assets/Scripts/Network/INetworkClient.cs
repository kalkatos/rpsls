using System;
using Kalkatos.Network.Model;

namespace Kalkatos.Network
{
	public interface INetworkClient
	{
		event Action<byte, object> OnEventReceived;

		bool IsConnected { get; }
		bool IsInRoom { get; }
		PlayerInfo[] Players { get; }
		PlayerInfo MyInfo { get; }

		void Connect (object parameter, Action<object> onSuccess, Action<object> onError);
		void Get (byte key, object parameter, Action<object> onSuccess, Action<object> onError);
		void Post (byte key, object parameter, Action<object> onSuccess, Action<object> onError);
	}
}