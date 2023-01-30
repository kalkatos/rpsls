using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame
{
	public class TimedEventSequence : MonoBehaviour
	{
		[SerializeField] private TimedEvent[] events;
		[SerializeField] private bool multipleEvents;
		[SerializeField, ShowIf(nameof(multipleEvents))] private bool loop;
		[SerializeField, ShowIf(nameof(multipleEvents)), ShowIf(nameof(loop))] private int loopCount;

		public UnityEvent TimeoutEvent;
		[ShowIf(nameof(multipleEvents))] public UnityEvent AnyTimeoutEvent;
		[ShowIf(nameof(multipleEvents)), ShowIf(nameof(loop))] public UnityEvent FinalTimeoutEvent;

		private int currentEvent = 0;
		private int loopCounter;

		private void Awake ()
		{
			foreach (var ev in events)
			{
				ev.FinalTimeoutEvent.AddListener(HandleEventTimeout);
			}
		}

		public void Rewind ()
		{
			currentEvent = 0;
			loopCounter = 0;
			foreach (var ev in events)
				ev.Rewind();
		}

		public void StartTimer ()
		{
			events[currentEvent].StartTimer();
		}

		public void Stop (bool invoke = false)
		{
			events[currentEvent].Stop(invoke);
			Rewind();
			if (invoke)
			{
				TimeoutEvent?.Invoke();
				if (loop)
					FinalTimeoutEvent?.Invoke();
			}
		}

		private void HandleEventTimeout ()
		{
			AnyTimeoutEvent?.Invoke();
			currentEvent++;
			if (currentEvent >= events.Length)
			{
				currentEvent = 0;
				TimeoutEvent?.Invoke();
				if (loop)
				{
					if (loopCount == 0 || loopCounter < loopCount)
					{
						loopCounter++;
						events[currentEvent].StartTimer();
					}
					else
						FinalTimeoutEvent?.Invoke();
				}
			}
			else
				events[currentEvent].StartTimer();
		}
	}
}