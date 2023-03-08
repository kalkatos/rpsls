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
		[ShowIf(nameof(isBoolSignal)), SerializeField] private ValueBinding<bool>[] BoolValueBindings;
		[ShowIf(nameof(isIntSignal)), SerializeField] private ValueBinding<int>[] IntValueBindings;
		[ShowIf(nameof(isStringSignal)), SerializeField] private ValueBinding<string>[] StringValueBindings;
		[ShowIf(nameof(isFloatSignal)), SerializeField] private ValueBinding<float>[] FloatValueBindings;
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
			foreach (var item in BoolValueBindings)
				item.TreatValue(b);
		}

		private void HandleIntSignalEmitted (int value)
		{
			foreach (var item in IntValueBindings)
				item.TreatValue(value);
		}

		private void HandleStringSignalEmitted (string value)
		{
			foreach (var item in StringValueBindings)
				item.TreatValue(value);
		}

		private void HandleFloatSignalEmitted (float value)
		{
			foreach (var item in FloatValueBindings)
				item.TreatValue(value);
		}
	}
}