using Kalkatos.Network.Unity;
using Kalkatos.UnityGame.Signals;
using UnityEngine;

namespace Kalkatos.UnityGame.NetworkBySignals
{
	[CreateAssetMenu(fileName = "MatchLeaver", menuName = "Network/Match Leaver")]
	public class MatchLeaver : ScriptableObject
	{
		[SerializeField] private Signal onSuccess;
		[SerializeField] private Signal onFailure;

		public void LeaveMatch ()
		{
			Logger.Log("Finding match...");
			NetworkClient.LeaveMatch(
				(success) =>
				{
					Logger.Log("Success leaving match.");
					onSuccess?.Emit();
				},
				(failure) =>
				{
					Logger.Log("Failed to leave match.");
					onFailure?.Emit();
				});
		}
	}
}
