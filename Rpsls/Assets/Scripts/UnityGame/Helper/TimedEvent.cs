using UnityEngine;
using Kalkatos.UnityGame.Signals;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame
{
	public class TimedEvent : MonoBehaviour
	{
		[SerializeField] private float time;
		[SerializeField] private bool loop;
		[SerializeField, ShowIf(nameof(loop))] private int loopCount;
		[SerializeField] private Signal timeoutSignal;
		[SerializeField] private UnityEvent timeoutEvent;
		[SerializeField, ShowIf(nameof(loop))] private Signal loopEndedSignal;
		[SerializeField, ShowIf(nameof(loop))] private UnityEvent loopEndedEvent;

		private int loopCounter;
		private Coroutine currentWait;

		public bool Loop { get => loop; set => loop = value; }

		public static void Create (float time, Signal signal = null, UnityEvent ev = null)
		{
			GameObject timerGO = new GameObject("Timer");
			var timer = timerGO.AddComponent<TimedEvent>();
			timer.time = time;
			timer.timeoutSignal = signal;
			timer.timeoutEvent = ev;
			timer.StartTimer(time);
		}

		public void Rewind ()
		{
			loopCounter = 0;
		}

		public void StartTimer ()
		{
			StartTimer(time);
		}

		public void StartTimer (float time)
		{
			Rewind();
			RunTimer(time);
		}

		public void Stop (bool invoke = false)
		{
			if (currentWait != null)
				StopCoroutine(currentWait);
			Rewind();
			if (invoke)
			{
				timeoutSignal?.Emit();
				timeoutEvent?.Invoke();
			}
		}

		private void RunTimer (float time)
		{
			loopCounter++;
			currentWait = StartCoroutine(MonoExtensions.WaitCoroutine(time, HandleTimerEnded));
		}

		private void HandleTimerEnded ()
		{
			timeoutSignal?.Emit();
			timeoutEvent?.Invoke();
			if (loop)
			{
				if (Mathf.Approximately(time, 0))
					Logger.LogWarning($"[{nameof(TimedEvent)}] Time is equal to 0 and is set to loop.");
				else if (loopCount == 0 || loopCounter < loopCount)
					RunTimer(time);
				else
				{
					loopEndedSignal?.Emit();
					loopEndedEvent?.Invoke();
				}
			}
		}
	}
}