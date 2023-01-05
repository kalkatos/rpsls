using Kalkatos.UnityGame.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kalkatos.Game.Rpsls
{
	public class ConnectionScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _connectingText;
        [SerializeField] private TMP_Text _notConnectedErrorText;

		private void Awake ()
		{
			_connectingText.gameObject.SetActive(true);
			_notConnectedErrorText.gameObject.SetActive(false);

			ConnectionScreen.OnNotConnectedError += HandleNotConnectedError;
		}

		private void OnDestroy ()
		{
			ConnectionScreen.OnNotConnectedError -= HandleNotConnectedError;
		}

		private void HandleNotConnectedError ()
		{
			_connectingText.gameObject.SetActive(false);
			_notConnectedErrorText.gameObject.SetActive(true);
		}
	}
}