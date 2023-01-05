using Kalkatos.Network.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.UnityGame.Systems
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