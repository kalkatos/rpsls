using Kalkatos.UnityGame;
using UnityEngine;

namespace Kalkatos.Firecard.Unity
{
	public class MovementBehaviour : MonoBehaviour
    {
        [SerializeField] private FloatValueGetter speed;
        [SerializeField] private Transform origin;
        
        private Vector3? targetPosition;

		private void Update ()
		{
			if (targetPosition.HasValue)
            {
                Vector3 distance = targetPosition.Value - transform.position;
                float maxFrameSpeed = speed.GetValue() * Time.deltaTime;
				if (distance.magnitude > maxFrameSpeed)
					transform.position += Vector3.ClampMagnitude(distance, maxFrameSpeed);
                else
                {
                    transform.position = targetPosition.Value;
                    targetPosition = null;
                }
            }
		}

        public void MoveTo (Vector3 target)
        {
            targetPosition = target;
        }

		public void MoveTo (Transform targetTransform)
        {
			MoveTo(targetTransform.position);
        }

        public void MoveToOrigin ()
        {
            if (origin != null)
                MoveTo(origin);
        }
    }
}
