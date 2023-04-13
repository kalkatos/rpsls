using Kalkatos.Firecard.Unity;
using Kalkatos.UnityGame.Scriptable;
using Kalkatos.UnityGame.Scriptable.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
	public class CardAttackAnimation : MonoBehaviour
    {
        [Header("Config")]
        public string attackAnimationName;
        public string receiverAnimationName;

        [SerializeField] private SignalString receiveAttackAnimationSignal;
        [SerializeField] private SignalState moveState;
        [SerializeField] private SignalState winnerState;
        [SerializeField] private string winnerToPlayAnimation;
		[Header("References")]
		[SerializeField, ChildGameObjectsOnly] private Animator animator;
		[SerializeField, ChildGameObjectsOnly] private CardBehaviour cardBehaviour;

        private bool sendingAttack;

        private bool isPlayed => moveState.Value == cardBehaviour.CardType;

		private void Awake ()
		{
            receiveAttackAnimationSignal.OnSignalEmittedWithParam.AddListener(ReceiveAttack);
            winnerState.OnSignalEmittedWithParam.AddListener(HandleWinnerReceived);
		}

		private void OnDestroy ()
        {
			receiveAttackAnimationSignal.OnSignalEmittedWithParam.RemoveListener(ReceiveAttack);
			winnerState.OnSignalEmittedWithParam.RemoveListener(HandleWinnerReceived);
		}


		private void HandleWinnerReceived (string winner)
		{
			if (winner == winnerToPlayAnimation && isPlayed)
                SendAttack();
		}

		public void SendAttack ()
        {
            animator.Play(attackAnimationName);
        }

		public void ReceiveAttack (string animationName)
		{
            if (sendingAttack || !isPlayed)
                return;
            animator.Play(animationName);
		}

        // Invoked by animation handle
        public void SendAttackReceived ()
        {
            sendingAttack = true;
			receiveAttackAnimationSignal?.EmitWithParam(receiverAnimationName);
            sendingAttack = false;
        }
    }
}