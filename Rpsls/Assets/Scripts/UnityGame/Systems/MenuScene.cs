using System;
using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Systems
{
	public class MenuScene : MonoBehaviour
	{
		

		private static void FindMatch ()
		{
			Logger.Log("Finding match...");
			NetworkClient.FindMatch(
				(success) =>
				{
					Logger.Log("Success finding match.");
				},
				(failure) =>
				{
					Logger.Log("Failed to find match.");
				});
		}
	}
}
