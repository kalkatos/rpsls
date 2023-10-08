// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if KALKATOS_NETWORK

using UnityEngine;
using UnityEngine.Events;
using Kalkatos.Network.Unity;
using Kalkatos.Network.Model;
using System.Collections.Generic;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "NetworkClientSO", menuName = "Network/Network Client")]
	public class NetworkClientScriptable : ScriptableObject
	{
		public StateBuilder StateBuilder;
		public PlayerDataBuilder PlayerDataBuilder;
		public UnityEvent OnNotConnected;
		public UnityEvent OnReconnected;
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

		private bool isConnected = true;

		public void SetNickname (string nick)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				SetAsNotConnected();
				return;
			}
			SetAsConnected();
			NetworkClient.SetNickname(nick);
		}

		public void SetPlayerData (SignalState playerDataChange)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				SetAsNotConnected();
				return;
			}
			SetAsConnected();
			NetworkClient.SetPlayerData(new Dictionary<string, string>() { { playerDataChange.Key, playerDataChange.Value } }, null, null);
		}

		public void SetPlayerData (string key)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				SetAsNotConnected();
				return;
			}
			SetAsConnected();
			if (PlayerDataBuilder == null || PlayerDataBuilder.MyInfo.OtherData == null)
				return;
			if (key == "Nickname")
			{
				NetworkClient.SetPlayerData(new Dictionary<string, string>() { { key, PlayerDataBuilder.MyInfo.Nickname.Value } }, 
					(success) => PlayerDataBuilder.MyInfo.Nickname.EmitWithParam(success.Nickname), 
					null);
				return;
			}
			foreach (var item in PlayerDataBuilder.MyInfo.OtherData)
			{
				if (item.Key == key)
				{
					NetworkClient.SetPlayerData(new Dictionary<string, string>() { { key, item.Value } }, null, null);
					return;
				}
			}
			Logger.LogWarning($"[{nameof(NetworkClientScriptable)}] Player data signal with key {key} is not set in the player data builder {PlayerDataBuilder.name}.");
		}

		public void UpdateConnection ()
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
				SetAsNotConnected();
			else
				SetAsConnected();
		}

		public void Connect ()
		{
			NetworkClient.Connect(
				(success) =>
				{
					SetAsConnected();
					PlayerDataBuilder?.UpdatePlayerData();
					OnConnectSuccess?.Invoke();
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						SetAsNotConnected();
					else
						OnConnectFailure?.Invoke();
				});
		}

		public void FindMatch ()
		{
			NetworkClient.FindMatch(
				(success) =>
				{ 
					SetAsConnected(); 
					OnFindMatchSuccess?.Invoke(); 
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						SetAsNotConnected();
					else
						OnFindMatchFailure?.Invoke();
				});
		}

		public void GetMatch ()
		{
			NetworkClient.GetMatch(
				(success) =>
				{
					SetAsConnected();
					PlayerDataBuilder?.UpdateOtherPlayersData();
					OnGetMatchSuccess?.Invoke();
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						SetAsNotConnected();
					else
						OnGetMatchFailure?.Invoke();
				});
		}

		public void LeaveMatch ()
		{
			NetworkClient.LeaveMatch(
				(success) =>
				{
					SetAsConnected();
					OnLeaveMatchSuccess?.Invoke();
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						SetAsNotConnected();
					else
						OnLeaveMatchFailure?.Invoke();
				});
		}

		public void SendAction ()
		{
			ActionInfo action = StateBuilder.BuildChangedPieces(NetworkClient.StateInfo);
            NetworkClient.SendAction(action,
				(success) =>
				{
					SetAsConnected();
					OnSendActionSuccess?.Invoke();
                },
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						SetAsNotConnected();
					else
                            OnSendActionFailure?.Invoke();
				});
		}

		public void GetMatchState ()
		{
			NetworkClient.GetMatchState(
				(success) =>
				{
					SetAsConnected();
					OnGetMatchStateSuccess?.Invoke();
					StateBuilder.ReceiveState(success);
				},
				(failure) => 
				{ 
					if (failure.Tag == NetworkErrorTag.NotConnected) 
						SetAsNotConnected(); 
					else 
						OnGetMatchStateFailure?.Invoke(); 
				});
		}

		private void SetAsConnected ()
		{
			if (!isConnected)
			{
				isConnected = true;
				OnReconnected?.Invoke();
			}
		}

		private void SetAsNotConnected ()
		{
			if (isConnected)
			{
				isConnected = false;
				OnNotConnected?.Invoke();
			}
		}
	}
}

#endif