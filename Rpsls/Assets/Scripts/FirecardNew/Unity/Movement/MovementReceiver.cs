// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using UnityEngine;

namespace Kalkatos.Firecard.Unity
{
    /// <summary>
    /// Receives a movement and positions objects in relation to itself.
    /// </summary>
	public class MovementReceiver : MonoBehaviour
    {
        [SerializeField] private bool canReceiveMovements = true;
        [SerializeField] private MovementHandling handling;

        private enum MovementHandling { MoveToCenter, Stack }

        public void ReceiveMovement (Transform receivedTransform)
        {
            if (canReceiveMovements)
            {
                if (!receivedTransform.TryGetComponent(out MovementBehaviour movementBehaviour))
                    movementBehaviour = receivedTransform.gameObject.AddComponent<MovementBehaviour>();
                switch (handling)
                {
                    case MovementHandling.MoveToCenter:
                        movementBehaviour.MoveTo(transform);
                        break;
                    case MovementHandling.Stack:
                        // TODO Receive movement and stack
                        break;
                }
            }
        }

        public void SetReceivingStatus (bool b)
        {
            canReceiveMovements = b;
        }

    }
}
