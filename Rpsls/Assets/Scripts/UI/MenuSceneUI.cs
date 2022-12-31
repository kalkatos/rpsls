using Kalkatos.UnityGame.Systems;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.Game.Rpsls
{
	public class MenuSceneUI : MonoBehaviour
	{
		[SerializeField] private Button _playGameButton;

		private void Awake ()
		{
			_playGameButton.onClick.AddListener(OnPlayGameButtonPressed);
		}

		private void OnPlayGameButtonPressed ()
		{
			MenuScene.FindMatch();
		}
	}
}