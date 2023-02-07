using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame.Scriptable
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
		[OnValueChanged(nameof(VerifySignal)), SerializeField] private Signal signal;
		[HideIf(nameof(isAnyOtherTypedSignal)), SerializeField] private UnityEvent action;
		[ShowIf(nameof(isBoolSignal)), SerializeField] private UnityEvent<bool> actionBool;
		[ShowIf(nameof(isIntSignal)), SerializeField] private UnityEvent<int> actionInt;
		[ShowIf(nameof(isStringSignal)), SerializeField] private UnityEvent<string> actionString;
		[ShowIf(nameof(isFloatSignal)), SerializeField] private UnityEvent<float> actionFloat;
		[HideInInspector, SerializeField] private bool isAnyOtherTypedSignal;
		[HideInInspector, SerializeField] private bool isBoolSignal;
		[HideInInspector, SerializeField] private bool isIntSignal;
		[HideInInspector, SerializeField] private bool isStringSignal;
		[HideInInspector, SerializeField] private bool isFloatSignal;

		public void Initialize ()
		{
			if (signal is TypedSignal<bool>)
				((TypedSignal<bool>)signal).OnSignalEmittedWithParam.AddListener(HandleBoolSignalEmitted);
			else if (signal is TypedSignal<int>)
				((TypedSignal<int>)signal).OnSignalEmittedWithParam.AddListener(HandleIntSignalEmitted);
			else if (signal is TypedSignal<string>)
				((TypedSignal<string>)signal).OnSignalEmittedWithParam.AddListener(HandleStringSignalEmitted);
			else if (signal is TypedSignal<float>)
				((TypedSignal<float>)signal).OnSignalEmittedWithParam.AddListener(HandleFloatSignalEmitted);
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
			else if (signal is TypedSignal<float>)
				((TypedSignal<float>)signal).OnSignalEmittedWithParam.RemoveListener(HandleFloatSignalEmitted);
			else
				signal?.OnSignalEmitted.RemoveListener(HandleSignalEmitted);
		}

		private void VerifySignal ()
		{
			isAnyOtherTypedSignal = false;
			isBoolSignal = signal != null && signal is TypedSignal<bool>;
			isIntSignal = signal != null && signal is TypedSignal<int>;
			isStringSignal = signal != null && signal is TypedSignal<string>;
			isFloatSignal = signal != null && signal is TypedSignal<float>;
			isAnyOtherTypedSignal = isBoolSignal || isIntSignal || isStringSignal || isFloatSignal;
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

		private void HandleFloatSignalEmitted (float value)
		{
			actionFloat?.Invoke(value);
		}
	}
}