using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable;
using Kalkatos.UnityGame.Scriptable.Network;
using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
    public class CardsManager : MonoBehaviour
    {
		public static CardsManager Instance;

        [SerializeField] private MovementBehaviour rockCard;
        [SerializeField] private MovementBehaviour paperCard;
        [SerializeField] private MovementBehaviour scissorsCard;
        [SerializeField] private MovementReceiver playCardTarget;
		[SerializeField] private SignalState moveSignal;
		[SerializeField] private SignalBool canPlay;

		private Transform lastUsedCard = null;

		private void Awake ()
		{
			Instance = this;
		}

        public void DragEndedOnCard (Transform card)
        {
            if (card != lastUsedCard)
                card?.gameObject.GetComponent<MovementBehaviour>()?.MoveToOrigin();
		}

        public void ReturnAllToOrigin ()
        {
			rockCard.MoveToOrigin();
			paperCard.MoveToOrigin();
			scissorsCard.MoveToOrigin();
			lastUsedCard = null;
		}

        public void UseCard (Transform cardTransform)
		{
			UseCard(cardTransform.GetComponent<CardBehaviour>(), cardTransform.GetComponent<MovementBehaviour>());
		}

		public void UseCard (CardBehaviour card, MovementBehaviour movement)
		{
			if (!canPlay.Value)
				return;
			lastUsedCard = card.transform;
			moveSignal.EmitWithParam(card.CardType);
			if (movement != rockCard)
				rockCard.MoveToOrigin();
			if (movement != paperCard)
				paperCard.MoveToOrigin();
			if (movement != scissorsCard)
				scissorsCard.MoveToOrigin();
			playCardTarget.ReceiveMovement(card.transform);
			card.BeUsed();
		}
	}
}
