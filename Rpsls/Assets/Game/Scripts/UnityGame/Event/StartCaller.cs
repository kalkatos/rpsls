using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame
{
	public class StartCaller : MonoBehaviour
	{
		[SerializeField] public UnityEvent onStartEvent;

		private void Start ()
		{
			onStartEvent?.Invoke();
		}
	}
}