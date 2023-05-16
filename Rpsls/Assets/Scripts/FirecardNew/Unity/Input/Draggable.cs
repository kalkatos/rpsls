// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kalkatos.Firecard.Unity
{
	/// <summary>
	/// MonoBehaviour to be used for any world object that can be moved by dragging. The object needs a collider and the camera needs a PhysicsRaycaster.
	/// </summary>
	public class Draggable : MonoBehaviour, IBeginDragHandler, IPointerDownHandler, IDragHandler, IEndDragHandler
	{
		[FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<PointerEventData> OnPointerDownEvent;
		[FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<PointerEventData> OnBeginDragEvent;
		[FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<PointerEventData> OnDragEvent;
		[FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<PointerEventData> OnEndDragEvent;

		[SerializeField] private bool canBeDragged = true;
		[SerializeField] private bool useDragPlane;
		[SerializeField] private InteractionSpace interactionSpace;
		[SerializeField, ShowIf(nameof(interactionSpace), InteractionSpace.UI)] private Image raycastImage;
		[SerializeField, ShowIf(nameof(interactionSpace), InteractionSpace.World)] private Collider dragCollider;
		[SerializeField] private bool useTilt;
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private Transform tiltTransform;
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private float tiltRate; 
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private float maxTiltAngle;
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt"), Range(0f, 1f)] private float tiltDamp;

		public enum InteractionSpace { World, UI }

		private Plane dragPlane;
		private Vector3 dragOffset;
		private Vector2 dragDelta;
		private Vector3 targetTilt = Vector3.back;
		private Coroutine correctDraggableCoroutine;

		private void Awake ()
		{
			if (tiltTransform == null)
				tiltTransform = transform;
		}

		private void Start ()
		{
			if (useDragPlane && DragPlane.Instance != null)
			{
				Transform dragPlaneTransform = DragPlane.Instance.transform;
				dragPlane = new Plane(dragPlaneTransform.up, dragPlaneTransform.position);
			}
			else
				useDragPlane = false;
		}

		private void Update ()
		{
			if (useTilt)
			{
				Vector3 velocity = Vector3.zero;
				tiltTransform.forward = Vector3.SmoothDamp(tiltTransform.forward, targetTilt, ref velocity, tiltDamp);
				targetTilt = -(dragPlane.normal + tiltRate / 100f * Vector3.ClampMagnitude(tiltTransform.TransformDirection(dragDelta), maxTiltAngle));
			}
		}

		public void SetDragStatus (bool b)
		{
			canBeDragged = b;
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			if (!canBeDragged)
				return;
			if (!useDragPlane)
				dragPlane = new Plane(-eventData.pressEventCamera.transform.forward, transform.position);
			Vector3 cameraPos = eventData.pressEventCamera.transform.position;
			Ray objRay = new Ray(cameraPos, transform.position - cameraPos);
			if (dragPlane.Raycast(objRay, out float distance))
				dragOffset = GetEventWorldPosition(eventData) - objRay.GetPoint(distance);
			OnPointerDownEvent?.Invoke(eventData);
		}

		public void OnBeginDrag (PointerEventData eventData)
		{
			if (!canBeDragged)
				return;
			correctDraggableCoroutine = StartCoroutine(CorrectDraggable());
			if (raycastImage != null)
				raycastImage.raycastTarget = false;
			if (dragCollider != null)
				dragCollider.enabled = false;
			OnBeginDragEvent?.Invoke(eventData);
		}

		public void OnDrag (PointerEventData eventData)
		{
			if (!canBeDragged)
				return;
			transform.position = GetEventWorldPosition(eventData) - dragOffset;
			dragDelta = eventData.delta;
			OnDragEvent?.Invoke(eventData);
		}

		public void OnEndDrag (PointerEventData eventData)
		{
			if (!canBeDragged)
				return;
			if (correctDraggableCoroutine != null)
			{
				StopCoroutine(correctDraggableCoroutine);
				correctDraggableCoroutine = null;
			}
			dragDelta = Vector2.zero;
			if (raycastImage != null)
				raycastImage.raycastTarget = true;
			if (dragCollider != null)
				dragCollider.enabled = true;
			OnEndDragEvent?.Invoke(eventData);
		}

		private Vector3 GetEventWorldPosition (PointerEventData eventData)
		{
			Ray eventRay = eventData.pressEventCamera.ScreenPointToRay(eventData.position);
			if (dragPlane.Raycast(eventRay, out float distance))
				return eventRay.GetPoint(distance);
			return Vector3.zero;
		}

		private IEnumerator CorrectDraggable ()
		{
			while (dragOffset.sqrMagnitude > 0.2f)
			{
				dragOffset = Vector3.Lerp(dragOffset, Vector3.zero, 0.1f);
				yield return null;
			}
		}
	}
}