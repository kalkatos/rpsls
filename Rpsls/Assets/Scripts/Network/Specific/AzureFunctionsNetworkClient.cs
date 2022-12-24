using System;
using Kalkatos.Network.Model;

namespace Kalkatos.Network.Specific
{
	public class AzureFunctionsNetworkClient : INetworkClient
	{
		public event Action<byte, object> OnEventReceived;

		public bool IsConnected { get; set; }
		public bool IsInRoom { get; set; }
		public PlayerInfo[] Players { get; set; }
		public PlayerInfo MyInfo { get; set; }


		public void Connect (object parameter, Action<object> onSuccess, Action<object> onError)
		{
			
		}

		public void Get (byte key, object parameter, Action<object> onSuccess, Action<object> onError)
		{
			
		}

		public void Post (byte key, object parameter, Action<object> onSuccess, Action<object> onError)
		{
			
		}
	}
}