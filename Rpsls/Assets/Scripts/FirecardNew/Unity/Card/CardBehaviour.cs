// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using Kalkatos.UnityGame.Rps;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.Firecard.Unity
{
	/// <summary>
	/// Unity MonoBehaviour for storing card data and invoking UnityEvents.
	/// </summary>
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