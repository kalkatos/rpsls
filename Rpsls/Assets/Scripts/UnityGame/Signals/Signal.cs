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
			OnSignalEmitted?.Invoke();
		}
	}

	public abstract class TypedSignal<T> : Signal
	{
		public UnityEvent<T> OnSignalEmittedWithParam;
		public virtual void EmitWithParam (T param) 
		{
			OnSignalEmittedWithParam?.Invoke(param);
		}
	}
}