using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame.Rps
{
	public class HighlightController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SignalBool canPlaySignal;
        [Header("References")]
        [SerializeField, ChildGameObjectsOnly] private SpriteRenderer highlight;
        [SerializeField, ChildGameObjectsOnly] private CardBehaviour cardBehaviour;
        [SerializeField, ChildGameObjectsOnly] private MovementBehaviour movementBehaviour;

		private void Awake ()
		{
            cardBehaviour.OnCardUsed.AddListener(OnCardUsed);
            movementBehaviour.OnMoved.AddListener(OnMoved);
            canPlaySignal.OnSignalEmittedWithParam.AddListener(OnCanPlay);
		}

		private void OnCanPlay (bool value)
		{
            highlight.enabled = value;
		}

		private void OnMoved (Transform destination)
		{
            if (destination.name.Contains("Origin"))
                highlight.enabled = canPlaySignal.Value;
		}

		private void OnCardUsed ()
        {
            highlight.enabled = false;
        }
	}
}