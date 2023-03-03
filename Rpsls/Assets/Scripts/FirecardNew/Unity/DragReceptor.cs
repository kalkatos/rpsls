using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kalkatos.Firecard.Unity
{
	public class DragReceptor : MonoBehaviour, IDropHandler
	{
		public UnityEvent OnDropReceived;

		public void OnDrop (PointerEventData eventData)
		{
			OnDropReceived?.Invoke();
			Debug.Log($"Drop!  LastPress = {eventData.lastPress?.name} , PointerPress = {eventData.pointerPress?.name} , PointerEnter = {eventData.pointerEnter?.name} , PointerClick = {eventData.pointerClick?.name} , RawPointerPress = {eventData.rawPointerPress?.name} , Selected Object = {eventData.selectedObject?.name}");
			StartCoroutine(MoveToZero(eventData.pointerCurrentRaycast.worldPosition));
		}

		private IEnumerator MoveToZero (Vector3 targetPosition) 
		{
			float time = 0.3f;
			float elapsedTime = 0;
			Vector3 startPosition = transform.localPosition;
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
