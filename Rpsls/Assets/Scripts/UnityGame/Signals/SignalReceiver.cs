using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Signals
{
	public class SignalReceiver : MonoBehaviour
	{
		[SerializeField] private Signal signal;
		[SerializeField] private UnityEvent action;

		private void Awake ()
		{
			signal.OnSignalEmitted.AddListener(HandleSignalEmitted);
		}

		private void OnDestroy ()
		{
			signal.OnSignalEmitted.RemoveListener(HandleSignalEmitted);
		}

		private void HandleSignalEmitted ()
		{
			action?.Invoke();
		}
	}
}