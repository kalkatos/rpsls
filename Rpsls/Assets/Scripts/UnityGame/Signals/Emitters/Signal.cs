using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignal", menuName = "Signals/Signal ()", order = 0)]
	public class Signal : ScriptableObject
	{
		public UnityEvent OnSignalEmitted;

		public virtual void Emit ()
		{
			if (SignalsSettings.Instance.EmitDebug)
				Logger.Log($"[Signal] {name} emitted.");
			OnSignalEmitted?.Invoke();
		}
	}

	public abstract class TypedSignal<T> : Signal
	{
		public UnityEvent<T> OnSignalEmittedWithParam;

		public T LastValue;

		public virtual void EmitWithParam (T param) 
		{
			if (SignalsSettings.Instance.EmitDebug)
				Logger.Log($"[TypedSignal] {name} emitted. Param = {param}");
			LastValue = param;
			OnSignalEmittedWithParam?.Invoke(param);
		}
	}
}