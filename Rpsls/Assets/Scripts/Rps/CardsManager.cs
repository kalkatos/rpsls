using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable.Network;
using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
    public class CardsManager : MonoBehaviour
    {
        [SerializeField] private Transform rockCard;
        [SerializeField] private Transform paperCard;
        [SerializeField] private Transform scissorsCard;
		[SerializeField] private SignalState moveSignal;

		private Transform lastDroppedCard = null;

        public void DropReceivedGoBackToOrigin (Transform droppedCard)
        {
			lastDroppedCard = droppedCard;
            if (droppedCard == rockCard)
				moveSignal.EmitWithParam("ROCK");
            else
                rockCard.gameObject.GetComponent<MovementBehaviour>()?.MoveToOrigin();
			if (droppedCard == paperCard)
				moveSignal.EmitWithParam("PAPER");
            else
			    paperCard.gameObject.GetComponent<MovementBehaviour>()?.MoveToOrigin();
			if (droppedCard == scissorsCard)
				moveSignal.EmitWithParam("SCISSORS");
            else
			    scissorsCard.gameObject.GetComponent<MovementBehaviour>()?.MoveToOrigin();
		}

        public void DragEndedOnCard (Transform card)
        {
            if (card != lastDroppedCard)
                card?.gameObject.GetComponent<MovementBehaviour>()?.MoveToOrigin();
		}
    }
}
