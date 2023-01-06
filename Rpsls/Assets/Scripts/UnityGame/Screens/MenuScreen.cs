using System;
using UnityEngine;
using Kalkatos.UnityGame.Signals;

namespace Kalkatos.UnityGame.Screens
{
	public class MenuScreen : MonoBehaviour
	{
		[SerializeField] private Signal playButtonClickSignal;

		private void Awake ()
		{
			playButtonClickSignal.OnSignalEmitted += HandlePlayButtonClickedSignal;
		}

		private void OnDestroy ()
		{
			playButtonClickSignal.OnSignalEmitted -= HandlePlayButtonClickedSignal;
		}

		private void HandlePlayButtonClickedSignal ()
		{
			ScreenManager.EndScreen("Menu", "FindingMatch");
		}
	}
}
