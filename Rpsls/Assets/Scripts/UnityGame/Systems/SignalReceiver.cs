using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Systems
{
	public class SignalReceiver : MonoBehaviour
	{
		[SerializeField] private Signal signal;
		[SerializeField] private UnityEvent action;

		private void Awake ()
		{
			signal.OnSignalEmitted += HandleSignalEmitted;
		}

		private void OnDestroy ()
		{
			signal.OnSignalEmitted -= HandleSignalEmitted;
		}

		private void HandleSignalEmitted ()
		{
			action?.Invoke();
		}
	}
}