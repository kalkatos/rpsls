// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kalkatos.Firecard.Unity
{
	/// <summary>
	/// MonoBehaviour that receives a Drop event. Needs a collider and the camera needs a PhysicsRaycaster.
	/// </summary>
	public class DragReceptor : MonoBehaviour, IDropHandler
	{
		[PropertyOrder(2)] public UnityEvent<Transform> OnDropReceived;

		[PropertyOrder(1), SerializeField] private bool canReceiveDrop = true;

		public void SetDropStatus (bool b)
		{
			canReceiveDrop = b;
		}

		public void OnDrop (PointerEventData eventData)
		{
			if (!canReceiveDrop)
				return;
			OnDropReceived?.Invoke(eventData.pointerPress?.transform);
			eventData.pointerPress.SendMessage("ReceiveDrop", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
