using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalDelayed", menuName = "Signals/Signal (Delayed)", order = 8)]
	public class SignalDelayed : TypedSignal<float>
	{
		[PropertyOrder(2)] public UnityEvent DelayedEvent;

		public override void EmitWithParam (float seconds)
		{
			TimedEvent.Create(seconds, DelayedEvent);
		}

		public void EmitWithParam (string dateTimeOrSeconds)
		{
			Logger.Log($"[SignalDelayed] Starting delay for signal with parameter {dateTimeOrSeconds}");
			if (DateTime.TryParse(dateTimeOrSeconds, out DateTime dateTime))
			{
				dateTime = dateTime.ToUniversalTime();
				DateTime now = DateTime.UtcNow;
				if (dateTime > now)
				{
					Value = (float)(dateTime - now).TotalSeconds;
					Logger.Log($"[SignalDelayed] Waiting seconds: {Value}");
					EmitWithParam(Value);
				}
				else
					DelayedEvent?.Invoke();
			}
			else if (float.TryParse(dateTimeOrSeconds, out float seconds))
			{
				Value = seconds;
				Logger.Log($"[SignalDelayed] Waiting seconds: {Value}");
				EmitWithParam(seconds); 
			}
		}
	}
}