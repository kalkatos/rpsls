using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Signals
{
	public class SignalReceiver : MonoBehaviour
	{
		[SerializeField] private SignalReceiverBit[] receivers;

		private void Awake ()
		{
			foreach (var item in receivers)
				item.Initialize();
		}

		private void OnDestroy ()
		{
			foreach (var item in receivers)
				item.Dispose();
		}
	}

	[Serializable]
	public class SignalReceiverBit
	{
		public Signal signal;
		public UnityEvent action;

		public void Initialize ()
		{
			signal.OnSignalEmitted.AddListener(HandleSignalEmitted);
		}

		public void Dispose ()
		{
			signal.OnSignalEmitted.RemoveListener(HandleSignalEmitted);
		}

		private void HandleSignalEmitted ()
		{
			action?.Invoke();
		}
	}
}