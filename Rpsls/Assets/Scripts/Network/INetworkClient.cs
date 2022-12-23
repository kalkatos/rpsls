using System;

namespace Kalkatos.Network
{
	public interface INetworkClient
	{
		event Action<byte, object> OnEventReceived;

		bool IsConnected { get; protected set; }
		bool IsInRoom { get; protected set; }
		PlayerInfo[] Players { get; protected set; }
		PlayerInfo MyInfo { get; protected set; }

		void Connect (object parameter, Action<object> onSuccess, Action<object> onError);
		void Get (byte key, object parameter, Action<object> onSuccess, Action<object> onError);
		void Post (byte key, object parameter, Action<object> onSuccess, Action<object> onError);
	}

	public class PlayerInfo
	{
		public string PlayerId;
		public string Nickname;
		public string[] CustomData;
	}
}