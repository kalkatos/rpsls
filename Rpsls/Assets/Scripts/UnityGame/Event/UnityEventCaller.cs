using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame
{
	public class UnityEventCaller : MonoBehaviour
	{
		[SerializeField] private EventCaller[] callbacks;

		private Dictionary<EventType, List<EventCaller>> callerMap = new Dictionary<EventType, List<EventCaller>>();

		private void Awake ()
		{
			foreach (var item in callbacks)
			{
				if (!callerMap.ContainsKey(item.Type))
					callerMap.Add(item.Type, new List<EventCaller>());
				callerMap[item.Type].Add(item);
			}
			ExecuteEventsOfType(EventType.Awake);
		}

		private void Start ()
		{
			ExecuteEventsOfType(EventType.Start);
		}

		private void Update ()
		{
			ExecuteEventsOfType(EventType.Update);
		}

		private void OnEnable ()
		{
			ExecuteEventsOfType(EventType.OnEnable);
		}

		private void OnDisable ()
		{
			ExecuteEventsOfType(EventType.OnDisable);
		}

		private void OnDestroy ()
		{
			ExecuteEventsOfType(EventType.OnDestroy);
		}

		private void ExecuteEventsOfType (EventType eventType)
		{
			if (callerMap.ContainsKey(eventType))
				foreach (var item in callerMap[eventType])
					item.Event.Invoke();
		}

		public enum EventType { Awake, Start, OnEnable, OnDisable, OnDestroy, Update }

		[Serializable]
		public class EventCaller
		{
			public EventType Type;
			public UnityEvent Event;
		}
	}
}