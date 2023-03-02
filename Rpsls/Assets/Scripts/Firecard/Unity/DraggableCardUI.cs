using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kalkatos.Firecard.Unity
{
	public class DraggableCardUI : MonoBehaviour, IBeginDragHandler, IPointerDownHandler, IDragHandler, IEndDragHandler
	{

		[SerializeField] private bool useDragPlane;
		[SerializeField] private Transform visualObject;
		[SerializeField] private bool useTilt;
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private float tiltRate;
		[SerializeField, ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private float maxTiltAngle;
		[SerializeField, Range(0f, 1f), ShowIf(nameof(useTilt)), FoldoutGroup("Tilt")] private float tiltDamp;

		private Plane dragPlane;
		private Vector3 dragOffset;
		private Vector2 dragDelta;
		private Vector3 targetTilt = Vector3.back;
		private Coroutine correctDraggableCoroutine;

		private void Awake ()
		{
			if (visualObject == null)
				visualObject = transform;
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
				visualObject.forward = Vector3.SmoothDamp(visualObject.forward, targetTilt, ref velocity, tiltDamp);
				targetTilt = -(dragPlane.normal + tiltRate / 100f * Vector3.ClampMagnitude(visualObject.TransformDirection(dragDelta), maxTiltAngle));
			}
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			if (!useDragPlane)
				dragPlane = new Plane(-eventData.pressEventCamera.transform.forward, transform.position);
			Vector3 cameraPos = eventData.pressEventCamera.transform.position;
			Ray objRay = new Ray(cameraPos, transform.position - cameraPos);
			if (dragPlane.Raycast(objRay, out float distance))
				dragOffset = GetEventWorldPosition(eventData) - objRay.GetPoint(distance);
		}

		public void OnBeginDrag (PointerEventData eventData)
		{
			correctDraggableCoroutine = StartCoroutine(CorrectDraggable());
		}

		public void OnDrag (PointerEventData eventData)
		{
			transform.position = GetEventWorldPosition(eventData) - dragOffset;
			dragDelta = eventData.delta;
		}

		public void OnEndDrag (PointerEventData eventData)
		{
			if (correctDraggableCoroutine != null)
			{
				StopCoroutine(correctDraggableCoroutine);
				correctDraggableCoroutine = null;
			}
			dragDelta = Vector2.zero;
			//DEBUG
			StartCoroutine(MoveToZero());
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

		// TEMP
		private IEnumerator MoveToZero ()
		{
			float time = 0.3f;
			float elapsedTime = 0;
			Vector3 startPosition = transform.localPosition;
			Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y, 0);
			while (elapsedTime < time)
			{
				elapsedTime += Time.deltaTime;
				transform.localPosition = Vector3.Lerp(startPosition, targetPosition, Mathf.Clamp01(elapsedTime / time));
				yield return null;
			}
			transform.localPosition = targetPosition;
		}
	}
}