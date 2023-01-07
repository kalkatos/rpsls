using Kalkatos.Network.Unity;
using UnityEngine;

namespace Kalkatos.UnityGame.Screens
{
	public class FindingMatchScreen : Screen
    {
		protected override void OnOpened ()
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
					screenSignal.EmitWithParam(false);
				});
		}
	}
}