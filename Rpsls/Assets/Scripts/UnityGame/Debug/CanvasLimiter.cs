using UnityEngine;

namespace Kalkatos.UnityGame.Debug
{
	[ExecuteAlways]
    public class CanvasLimiter : MonoBehaviour
    {
        [SerializeField] private Canvas worldCanvas;
		[SerializeField] private Vector2 maxAspectRatio;

		private void Update ()
		{
			RectTransform rectTransform = (RectTransform)worldCanvas.transform;
			float measuredAspectRatio = (float)Screen.width / Screen.height;
			rectTransform.sizeDelta = new Vector2(Mathf.Min(rectTransform.sizeDelta.y * measuredAspectRatio, rectTransform.sizeDelta.y * maxAspectRatio.x / maxAspectRatio.y), rectTransform.sizeDelta.y);
			//UnityEngine.Debug.Log($"{Screen.width}  {Screen.height}  {measuredAspectRatio}   {rectTransform.sizeDelta}");
		}
	}
}
