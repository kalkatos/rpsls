using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Kalkatos.UnityGame.Scriptable
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

		[PropertyOrder(1), FormerlySerializedAs ("LastValue")] public T Value;
		[Space(10)]
		[PropertyOrder(2), SerializeField] private ValueBinding<T>[] ValueBindings;

		public virtual void EmitWithParam (T param) 
		{
			if (SignalsSettings.Instance.EmitDebug)
				Logger.Log($"[TypedSignal] {name} emitted. Param = {param}");
			Value = param;
			OnSignalEmittedWithParam?.Invoke(param);
			if (ValueBindings != null)
				foreach (var item in ValueBindings)
					if (param.Equals(item.ExpectedValue))
						item.Event?.Invoke(param);
		}
	}

	[Serializable]
	public class ValueBinding<T>
	{
		public T ExpectedValue;
		public UnityEvent<T> Event;
	}
}