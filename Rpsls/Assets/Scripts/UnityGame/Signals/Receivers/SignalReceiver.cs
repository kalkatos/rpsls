using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

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
		[OnValueChanged(nameof(VerifySignal))] public Signal signal;
		[HideInInspector] public bool isAnyOtherTypedSignal;
		[HideIf(nameof(isAnyOtherTypedSignal))] public UnityEvent action;
		[HideInInspector] public bool isBoolSignal;
		[ShowIf(nameof(isBoolSignal))] public UnityEvent<bool> actionBool;
		[HideInInspector] public bool isStringSignal;
		[ShowIf(nameof(isStringSignal))] public UnityEvent<string> actionString;
		[HideInInspector] public bool isIntSignal;
		[ShowIf(nameof(isIntSignal))] public UnityEvent<int> actionInt;

		private void VerifySignal ()
		{
			isAnyOtherTypedSignal = false;
			isBoolSignal = signal != null && signal is TypedSignal<bool>;
			isAnyOtherTypedSignal |= isBoolSignal;
			isStringSignal = signal != null && signal is TypedSignal<string>;
			isAnyOtherTypedSignal |= isStringSignal;
			isIntSignal = signal != null && signal is TypedSignal<int>;
			isAnyOtherTypedSignal |= isIntSignal;
		}

		public void Initialize ()
		{
			if (signal is TypedSignal<bool>)
				((TypedSignal<bool>)signal).OnSignalEmittedWithParam.AddListener(HandleBoolSignalEmitted);
			else if (signal is TypedSignal<int>)
				((TypedSignal<int>)signal).OnSignalEmittedWithParam.AddListener(HandleIntSignalEmitted);
			else if (signal is TypedSignal<string>)
				((TypedSignal<string>)signal).OnSignalEmittedWithParam.AddListener(HandleStringSignalEmitted);
			else
				signal.OnSignalEmitted.AddListener(HandleSignalEmitted);
		}

		public void Dispose ()
		{
			if (signal is TypedSignal<bool>)
				((TypedSignal<bool>)signal)?.OnSignalEmittedWithParam.RemoveListener(HandleBoolSignalEmitted);
			else if (signal is TypedSignal<int>)
				((TypedSignal<int>)signal)?.OnSignalEmittedWithParam.RemoveListener(HandleIntSignalEmitted);
			else if (signal is TypedSignal<string>)
				((TypedSignal<string>)signal)?.OnSignalEmittedWithParam.RemoveListener(HandleStringSignalEmitted);
			else
				signal?.OnSignalEmitted.RemoveListener(HandleSignalEmitted);
		}

		private void HandleSignalEmitted ()
		{
			action?.Invoke();
		}

		private void HandleBoolSignalEmitted (bool b)
		{
			actionBool?.Invoke(b);
		}

		private void HandleIntSignalEmitted (int value)
		{
			actionInt?.Invoke(value);
		}

		private void HandleStringSignalEmitted (string value)
		{
			actionString?.Invoke(value);
		}
	}
}