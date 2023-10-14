using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;  
#endif
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Rps
{
	public class HighlightController : MonoBehaviour
    {
#if ODIN_INSPECTOR
		[PropertyOrder(99)]  
#endif
        public UnityEvent OnHighlightOn;
#if ODIN_INSPECTOR
		[PropertyOrder(99)]  
#endif
        public UnityEvent OnHighlightOff;

        [Header("Config")]
        [SerializeField] private SignalBool canPlaySignal;
        [Header("References")]
#if ODIN_INSPECTOR
		[ChildGameObjectsOnly] 
#endif
        [SerializeField] private CardBehaviour cardBehaviour;
#if ODIN_INSPECTOR
		[ChildGameObjectsOnly] 
#endif
        [SerializeField] private MovementBehaviour movementBehaviour;

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
			OnHighlightOn?.Invoke();
			isHighlighted = true;
		}

		private void HighlightOff (string reason)
		{
			if (!isHighlighted)
				return;
			OnHighlightOff?.Invoke();
			isHighlighted = false;
		}
	}
}