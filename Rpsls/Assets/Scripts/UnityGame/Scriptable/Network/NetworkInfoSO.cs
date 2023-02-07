using UnityEngine;
using UnityEngine.Events;
using Kalkatos.Network.Unity;
using Kalkatos.Network.Model;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "NetworkInfoSO", menuName = "Network/Network Info")]
	public class NetworkInfoSO : ScriptableObject
	{
		public UnityEvent<string> OnPlayerNicknameChanged;
		public UnityEvent<string> OnOpponentNicknameReceived;
		public UnityEvent<int> OnPlayerAvatarChanged;
		public UnityEvent<int> OnOpponentAvatarReceived;
		public UnityEvent<bool> IsInMatch;

		public void UpdateMatchStatus ()
		{
			if (!NetworkClient.IsConnected)
			{
				IsInMatch?.Invoke(false);
				return;
			}
			if (NetworkClient.MatchInfo == null)
			{
				IsInMatch?.Invoke(false);
				return; 
			}
			if (string.IsNullOrEmpty(NetworkClient.MatchInfo.MatchId))
			{
				IsInMatch?.Invoke(false);
				return; 
			}
			IsInMatch?.Invoke(true);
		}

		public void UpdatePlayerData ()
		{
			PlayerInfo myInfo = NetworkClient.MyInfo;
			if (myInfo != null)
			{
				string nickname = myInfo.Nickname;
				OnPlayerNicknameChanged?.Invoke(nickname);
				Logger.Log($"Player nickname: {nickname}");
				if (myInfo.CustomData != null && myInfo.CustomData.TryGetValue("Avatar", out string value))
				{
					int avatarIndex = int.Parse(value);
					OnPlayerAvatarChanged?.Invoke(avatarIndex);
					Logger.Log($"Player avatar: {avatarIndex}");
				}
			}
		}

		public void UpdateOpponentData ()
		{
			if (NetworkClient.MatchInfo != null)
			{
				foreach (var item in NetworkClient.MatchInfo.Players)
				{
					if (item.Alias != NetworkClient.MyInfo.Alias)
					{
						string opponentNickname = item.Nickname;
						OnOpponentNicknameReceived?.Invoke(opponentNickname);
						Logger.Log($"Received opponent nickname: {opponentNickname}");
						if (item.CustomData != null && item.CustomData.TryGetValue("Avatar", out string value))
						{
							int avatarIndex = int.Parse(value);
							OnOpponentAvatarReceived?.Invoke(avatarIndex);
							Logger.Log($"Opponent avatar: {avatarIndex}");
						}
						break;
					}
				}
			}
		}
	}
}