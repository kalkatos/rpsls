using Kalkatos.Network.Unity;
using UnityEngine;

namespace Kalkatos.UnityGame.Screens
{
	public class FindingMatchScreen : MonoBehaviour
    {
		
		private void Awake ()
		{
			ScreenManager.OnStarted += HandleScreenStarted;
		}

		private void OnDestroy ()
		{
			ScreenManager.OnStarted -= HandleScreenStarted;
		}

		private void HandleScreenStarted (string name)
		{
			if (name != "FindingMatch")
				return;
			Logger.Log("Finding match...");
			NetworkClient.FindMatch(
				(success) =>
				{
					Logger.Log("Success finding match.");
					ScreenManager.EndScreen("FindingMatch", "Game");
				},
				(failure) =>
				{
					Logger.Log("Failed to find match.");
					ScreenManager.EndScreen("FindingMatch", "Menu");
				});
		}
	}
}