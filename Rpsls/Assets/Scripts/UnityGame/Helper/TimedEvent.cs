using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;
using UnityEditor.PackageManager;

namespace Kalkatos.UnityGame
{
	public class TimedEvent : MonoBehaviour
	{
		[SerializeField] private TimedEventBit[] events;
		[SerializeField] private bool multipleEvents;
		[SerializeField, ShowIf(nameof(multipleEvents))] private bool loop;
		[SerializeField, ShowIf(nameof(multipleEvents)), ShowIf(nameof(loop))] private int loopCount;

		[ShowIf(nameof(multipleEvents))] public UnityEvent TimeoutEvent;
		[ShowIf(nameof(multipleEvents))] public UnityEvent AnyTimeoutEvent;
		[ShowIf(nameof(multipleEvents)), ShowIf(nameof(loop))] public UnityEvent FinalTimeoutEvent;

		private int currentEvent = 0;
		private int loopCounter;

		private void Awake ()
		{
			foreach (var ev in events)
			{
				ev.SetParent(this);
				ev.FinalTimeoutEvent.AddListener(HandleEventTimeout);
			}
		}

		public static void Create (float time, UnityEvent ev = null)
		{
			GameObject timerGO = new GameObject("Timer");
			var timer = timerGO.AddComponent<TimedEvent>();
			timer.AddTimer(time, ev);
		}

		public void AddTimer (float time, UnityEvent ev)
		{
			TimedEventBit[] newEvents = new TimedEventBit[events.Length + 1];
			for (int i = 0; i < events.Length; i++)
				newEvents[i] = events[i];
			TimedEventBit newBit = new TimedEventBit(this)
			{
				time = time,
				TimeoutEvent = ev
			};
			newEvents[newEvents.Length - 1] = newBit;
			events = newEvents;
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

	[Serializable]
	public class TimedEventBit
	{
		public float time;
		public bool loop;
		[ShowIf(nameof(loop))] public int loopCount;

		public UnityEvent TimeoutEvent;
		[ShowIf(nameof(loop))] public UnityEvent FinalTimeoutEvent;

		private int loopCounter;
		private Coroutine currentWait;
		private TimedEvent parent;

		public bool Loop { get => loop; set => loop = value; }

		public TimedEventBit (TimedEvent parent)
		{
			SetParent(parent);
		}

		public void SetParent (TimedEvent timedEvent)
		{
			parent = timedEvent;
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
				parent.StopCoroutine(currentWait);
			Rewind();
			if (invoke)
			{
				TimeoutEvent?.Invoke();
			}
		}

		private void RunTimer (float time)
		{
			loopCounter++;
			currentWait = parent.StartCoroutine(MonoExtensions.WaitCoroutine(time, HandleTimerEnded));
		}

		private void HandleTimerEnded ()
		{
			TimeoutEvent?.Invoke();
			if (loop)
			{
				if (Mathf.Approximately(time, 0))
					Logger.LogWarning($"[{nameof(TimedEvent)}] Time is equal to 0 and is set to loop.");
				else if (loopCount == 0 || loopCounter < loopCount)
					RunTimer(time);
				else
				{
					FinalTimeoutEvent?.Invoke();
				}
			}
		}
	}
}