using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kalkatos.Firecard.Unity
{
	public class DragReceptor : MonoBehaviour, IDropHandler
	{
		public UnityEvent<Transform> OnDropReceived;

		public void OnDrop (PointerEventData eventData)
		{
			OnDropReceived?.Invoke(eventData.pointerPress?.transform);
			eventData.pointerPress.SendMessage("ReceiveDrop", this, SendMessageOptions.DontRequireReceiver);
			Debug.Log($"Drop!");//  LastPress = {eventData.lastPress?.name} , PointerPress = {eventData.pointerPress?.name} , PointerEnter = {eventData.pointerEnter?.name} , PointerClick = {eventData.pointerClick?.name} , RawPointerPress = {eventData.rawPointerPress?.name} , Selected Object = {eventData.selectedObject?.name}");
		}
	}
}
