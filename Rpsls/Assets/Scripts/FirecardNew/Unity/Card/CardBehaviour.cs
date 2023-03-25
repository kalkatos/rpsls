using Kalkatos.UnityGame.Rps;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.Firecard.Unity
{
	public class CardBehaviour : MonoBehaviour
	{
		[PropertyOrder(99)] public UnityEvent OnCardUsed;

		public string CardType;

		public void Use ()
		{
			CardsManager.Instance.UseCard(transform);
		}

		public void BeUsed ()
		{
			OnCardUsed.Invoke();
		}
	}
}