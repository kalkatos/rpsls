using Kalkatos.Network.Unity;
using Kalkatos.UnityGame.Signals;
using UnityEngine;

namespace Kalkatos.UnityGame.NetworkBySignals
{
	[CreateAssetMenu(fileName = "MatchFinder", menuName = "Network/Match Finder")]
	public class MatchFinder : ScriptableObject
    {
		[SerializeField] private Signal onSuccess;
		[SerializeField] private Signal onFailure;

		public void FindMatch ()
		{
			Logger.Log("Finding match...");
			NetworkClient.FindMatch(
				(success) =>
				{
					Logger.Log("Success finding match.");
					onSuccess?.Emit();
				},
				(failure) =>
				{
					Logger.Log("Failed to find match.");
					onFailure?.Emit();
				});
		}
	}
}