using UnityEngine;
using Kalkatos.UnityGame.Signals;
using UnityEngine.Events;

namespace Kalkatos.UnityGame
{
	public class TimedEvent : MonoBehaviour
	{
		[SerializeField] private float time;
		[SerializeField] private Signal timeoutSignal;
		[SerializeField] private UnityEvent timeoutEvent;

		public static void Create (float time, Signal signal = null, UnityEvent ev = null)
		{
			GameObject timerGO = new GameObject("Timer");
			var timer = timerGO.AddComponent<TimedEvent>();
			timer.time = time;
			timer.timeoutSignal = signal;
			timer.timeoutEvent = ev;
			timer.StartTimer(time);
		}

		public void StartTimer ()
		{
			this.Wait(time, HandleTimerEnded);
		}

		public void StartTimer (float time)
		{
			this.Wait(time, HandleTimerEnded);
		}

		private void HandleTimerEnded ()
		{
			timeoutSignal?.Emit();
			timeoutEvent?.Invoke();
		}
	}
}