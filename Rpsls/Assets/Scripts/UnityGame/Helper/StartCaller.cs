using UnityEngine;
using Kalkatos.UnityGame.Signals;
using UnityEngine.Events;

namespace Kalkatos.UnityGame
{
	public class StartCaller : MonoBehaviour
	{
		[SerializeField] public Signal onStartSignal;
		[SerializeField] public UnityEvent onStartEvent;

		private void Start ()
		{
			onStartSignal?.Emit();
			onStartEvent?.Invoke();
		}
	}
}