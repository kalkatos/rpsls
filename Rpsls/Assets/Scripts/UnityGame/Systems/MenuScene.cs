using System;
using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Systems
{
	public class MenuScene : MonoBehaviour
	{
		private static bool _isFindingMatch;

		private void Awake ()
		{
			_isFindingMatch = false;
		}

		public static void FindMatch ()
		{
			if (_isFindingMatch)
				return;
			_isFindingMatch = true;
			Logger.Log("Finding match...");
			NetworkClient.FindMatch(
				(success) =>
				{
					_isFindingMatch = false;
					Logger.Log("Success finding match.");
				},
				(failure) =>
				{
					_isFindingMatch = false;
					Logger.Log("Failed to find match.");
				});
		}
	}
}
