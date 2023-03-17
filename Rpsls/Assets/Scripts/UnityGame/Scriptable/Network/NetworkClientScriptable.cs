using UnityEngine;
using UnityEngine.Events;
using Kalkatos.Network.Unity;
using System.Collections.Generic;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "NetworkClientSO", menuName = "Network/Network Client")]
	public class NetworkClientScriptable : ScriptableObject
	{
		public StateBuilder StateBuilder;
		public PlayerDataBuilder PlayerDataBuilder;
		public UnityEvent OnNotConnected;
		public UnityEvent OnConnectSuccess;
		public UnityEvent OnConnectFailure;
		public UnityEvent OnFindMatchSuccess;
		public UnityEvent OnFindMatchFailure;
		public UnityEvent OnGetMatchSuccess;
		public UnityEvent OnGetMatchFailure;
		public UnityEvent OnLeaveMatchSuccess;
		public UnityEvent OnLeaveMatchFailure;
		public UnityEvent OnSendActionSuccess;
		public UnityEvent OnSendActionFailure;
		public UnityEvent OnGetMatchStateSuccess;
		public UnityEvent OnGetMatchStateFailure;

		public void SetNickname (string nick)
		{
			NetworkClient.SetNickname(nick);
		}

		public void SetPlayerData (SignalState playerDataChange)
		{
			NetworkClient.SetPlayerData(new Dictionary<string, string>() { { playerDataChange.Key, playerDataChange.Value } });
		}

		public void SetPlayerData (string key)
		{
			if (PlayerDataBuilder == null || PlayerDataBuilder.MyInfo.OtherData == null)
				return;
			foreach (var item in PlayerDataBuilder.MyInfo.OtherData)
			{
				if (item.Key == key)
				{
					NetworkClient.SetPlayerData(new Dictionary<string, string>() { { key, item.Value } });
					return;
				}
			}
			Logger.LogWarning($"[NetworkClientScriptable] Couldn't find player data with key {key}.");
		}

		public void Connect ()
		{
			NetworkClient.Connect(
				(success) =>
				{
					PlayerDataBuilder?.UpdatePlayerData();
					OnConnectSuccess?.Invoke();
				},
				(failure) =>
				{
					if (failure.Tag == Kalkatos.Network.Model.NetworkErrorTag.NotConnected)
						OnNotConnected?.Invoke();
					else
						OnConnectFailure?.Invoke();
				});
		}

		public void FindMatch ()
		{
			NetworkClient.FindMatch((success) => OnFindMatchSuccess?.Invoke(), (failure) => OnFindMatchFailure?.Invoke());
		}

		public void GetMatch ()
		{
			NetworkClient.GetMatch(
				(success) =>
				{
					PlayerDataBuilder?.UpdateOtherPlayersData();
					OnGetMatchSuccess?.Invoke();
				}, 
				(failure) => OnGetMatchFailure?.Invoke());
		}

		public void LeaveMatch ()
		{
			NetworkClient.LeaveMatch((success) => OnLeaveMatchSuccess?.Invoke(), (failure) => OnLeaveMatchFailure?.Invoke());
		}

		public void SendAction ()
		{
			NetworkClient.SendAction(StateBuilder.BuildChangedPieces(NetworkClient.StateInfo), 
				(success) => 
				{
					OnSendActionSuccess?.Invoke();
					StateBuilder.ReceiveState(success);
				}, 
				(failure) => OnSendActionFailure?.Invoke());
		}

		public void GetMatchState ()
		{
			NetworkClient.GetMatchState(
				(success) => 
				{
					OnGetMatchStateSuccess?.Invoke();
					StateBuilder.ReceiveState(success);
				}, 
				(failure) => OnGetMatchStateFailure?.Invoke());
		}
	}
}