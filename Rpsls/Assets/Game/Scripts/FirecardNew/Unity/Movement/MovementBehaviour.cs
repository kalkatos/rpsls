// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using Kalkatos.UnityGame;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.Firecard.Unity
{
    /// <summary>
    /// Manages movement of objects.
    /// </summary>
	public class MovementBehaviour : MonoBehaviour
    {
        [PropertyOrder(99)] public UnityEvent<Transform> OnMoved;
        [PropertyOrder(99)] public UnityEvent OnArrived;
        [PropertyOrder(99)] public UnityEvent OnMovedToOrigin;

        [SerializeField] private FloatValueGetter speed;
        [SerializeField] private Transform origin;
        
        private Vector3? targetPosition;
        private Action<Transform> callback;

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
                    OnArrived?.Invoke();
                    callback?.Invoke(transform);
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
            OnMoved?.Invoke(targetTransform);
        }

        public void MoveTo (Transform targetTransform, Action<Transform> arriveCallback)
        {
            callback = arriveCallback;
            MoveTo(targetTransform.position);
            OnMoved?.Invoke(targetTransform);
        }

        public void MoveToOrigin ()
        {
            if (origin != null)
            {
                MoveTo(origin);
                OnMovedToOrigin?.Invoke();
			}
        }
    }
}
