using System;
using System.ComponentModel;
using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignal", menuName = "Signal/Signal ()", order = 0)]
	public class Signal : ScriptableObject
	{
		public event Action OnSignalEmitted;

		public virtual void Emit ()
		{
			OnSignalEmitted?.Invoke();
		}
	}

	public abstract class TypedSignal<T> : Signal
	{
		public event Action<T> OnSignalEmittedWithParam;
		public virtual void EmitWithParam (T param) 
		{
			OnSignalEmittedWithParam?.Invoke(param);
		}
	}
}