using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignal", menuName = "Signals/Signal ()", order = 0)]
	public class Signal : ScriptableObject
	{
		public UnityEvent OnSignalEmitted;

		public virtual void Emit ()
		{
			Log();
			OnSignalEmitted?.Invoke();
		}

		protected virtual void Log ()
		{
			if (SignalsSettings.Instance.EmitDebug)
				Logger.Log($"[Signal] {name} emitted.");
		}
	}

	public abstract class TypedSignal<T> : Signal, IValueGetter<T>
	{
		public UnityEvent<T> OnSignalEmittedWithParam;

		[PropertyOrder(1)] public T Value;
		[Space(10)]
		[PropertyOrder(5), SerializeField] private ValueBinding<T>[] ValueBindings;

		public virtual void EmitWithParam (T param) 
		{
			Value = param;
			Log();
			OnSignalEmittedWithParam?.Invoke(param);
			if (ValueBindings != null)
				foreach (var item in ValueBindings)
					if ((item.Equality == Equality.Equals && param.Equals(item.ExpectedValue))
						|| (item.Equality == Equality.NotEquals && !param.Equals(item.ExpectedValue)))
						item.Event?.Invoke(param);
		}

		public T GetValue () => Value;

		protected override void Log ()
		{
			if (SignalsSettings.Instance.EmitDebug)
				Logger.Log($"[{GetType().Name}] {name} emitted. Value = {Value}");
		}
	}

	[Serializable]
	public class ValueBinding<T>
	{
		public T ExpectedValue;
		public Equality Equality;
		public UnityEvent<T> Event;
	}

	public enum Equality
	{
		Equals,
		NotEquals,
	}
}