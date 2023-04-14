using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Rps
{
	public class HighlightController : MonoBehaviour
    {
        [PropertyOrder(99)] public UnityEvent OnHighlightOn;
        [PropertyOrder(99)] public UnityEvent OnHighlightOff;

        [Header("Config")]
        [SerializeField] private SignalBool canPlaySignal;
        [Header("References")]
        [SerializeField, ChildGameObjectsOnly] private CardBehaviour cardBehaviour;
        [SerializeField, ChildGameObjectsOnly] private MovementBehaviour movementBehaviour;

		private bool isHighlighted;

		private void Awake ()
		{
            cardBehaviour.OnCardUsed.AddListener(OnCardUsed);
            movementBehaviour.OnMovedToOrigin.AddListener(OnMovedToOrigin);
            canPlaySignal.OnSignalEmittedWithParam.AddListener(OnCanPlay);
		}

		private void OnDestroy ()
		{
			cardBehaviour.OnCardUsed.RemoveListener(OnCardUsed);
			movementBehaviour.OnMovedToOrigin.RemoveListener(OnMovedToOrigin);
			canPlaySignal.OnSignalEmittedWithParam.RemoveListener(OnCanPlay);
		}

		private void OnCanPlay (bool value)
		{
			if (value)
				HighlightOn("Can Play");
			else
				HighlightOff("Can Play");
		}

		private void OnMovedToOrigin ()
		{
			if (canPlaySignal.Value)
				HighlightOn($"Moved to Origin");
			else
				HighlightOff($"Moved to Origin");
		}

		private void OnCardUsed ()
        {
			HighlightOff("Used");
		}

		private void HighlightOn (string reason)
		{
			if (isHighlighted)
				return;
			Logger.Log($"[Highlight {name}] ON : {reason}");
			OnHighlightOn?.Invoke();
			isHighlighted = true;
		}

		private void HighlightOff (string reason)
		{
			if (!isHighlighted)
				return;
			Logger.Log($"[Highlight {name}] OFF : {reason}");
			OnHighlightOff?.Invoke();
			isHighlighted = false;
		}
	}
}