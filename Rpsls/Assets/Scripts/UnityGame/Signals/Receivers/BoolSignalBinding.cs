using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Signals
{
	public class BoolSignalBinding : MonoBehaviour
	{
		[SerializeField] private SignalBool signal;
		[SerializeField] private UnityEvent<bool> paramEvent;
		[SerializeField] private UnityEvent<bool> notParamEvent;
		[SerializeField] private UnityEvent trueEvent;
		[SerializeField] private UnityEvent falseEvent;

		private void OnEnable ()
		{
			Logger.Log($"OnEnable of Signal {signal.name} : {signal.LastValue}");
			signal.OnSignalEmittedWithParam.AddListener(HandleSignalEmitted);
			paramEvent?.Invoke(signal.LastValue);
			notParamEvent?.Invoke(!signal.LastValue);
			if (signal.LastValue)
				trueEvent?.Invoke();
			else
				falseEvent?.Invoke();
		}

		private void OnDisable ()
		{
			signal.OnSignalEmittedWithParam.RemoveListener(HandleSignalEmitted);
		}

		private void HandleSignalEmitted (bool b)
		{
			Logger.Log($"EMITTED Signal {signal.name} : {signal.LastValue}");
			paramEvent?.Invoke(b);
			notParamEvent?.Invoke(!b);
			if (b)
				trueEvent?.Invoke();
			else
				falseEvent?.Invoke();
		}
	}
}