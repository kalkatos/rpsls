using System;

namespace Kalkatos.Network
{
	public interface IClientNetworkManager
	{
		bool IsConnected { get; protected set; }
		bool IsInRoom { get; protected set; }
		PlayerInfo[] Players { get; protected set; }
		PlayerInfo MyPlayerInfo { get; protected set; }
		RoomInfo CurrentRoomInfo { get; protected set; }
		void Connect (object parameter, Action<object> onSuccess, Action<object> onError);
		void FindMatch (object parameter, Action<object> onSuccess, Action<object> onError);
		void LeaveMatch (object parameter, Action<object> onSuccess, Action<object> onError);
		void RequestData (object parameter, Action<object> onSuccess, Action<object> onError);
		void SendData (object parameter, Action<object> onSuccess, Action<object> onError);
		void ExecuteFunction (object parameter, Action<object> onSuccess, Action<object> onError);
	}
}
