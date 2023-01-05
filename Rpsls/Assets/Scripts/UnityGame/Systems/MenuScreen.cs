using System;
using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Systems
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
