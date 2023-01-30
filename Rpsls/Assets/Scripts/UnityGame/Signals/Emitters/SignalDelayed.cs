using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignalDelayed", menuName = "Signals/Signal (Delayed)", order = 0)]
	public class SignalDelayed : Signal
	{
		public float Delay;
		public UnityEvent callbackEvent;

		public override void Emit ()
		{
			TimedEvent.Create(Delay, callbackEvent);
		}
	}
}