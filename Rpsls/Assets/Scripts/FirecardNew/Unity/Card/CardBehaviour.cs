using Kalkatos.UnityGame.Rps;
using UnityEngine;

namespace Kalkatos.Firecard.Unity
{
	public class CardBehaviour : MonoBehaviour
	{
		public string CardType;

		public void Use ()
		{
			CardsManager.Instance.UseCard(transform);
		}
	}
}