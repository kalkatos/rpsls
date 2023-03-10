using UnityEngine;

namespace Kalkatos.UnityGame
{
	[ExecuteAlways]
    public class CanvasLimiter : MonoBehaviour
    {
        [SerializeField] private Canvas worldCanvas;
		[SerializeField] private Vector2 maxAspectRatio;

		private void Start ()
		{
			Logger.Log($"Updating Canvas Rect (Start) {Screen.width}x{Screen.height}");
			UpdateCanvasRect();
		}

		private void OnRectTransformDimensionsChange ()
		{
			Logger.Log($"Updating Canvas Rect (Rect Transform Dimensions Change) {Screen.width}x{Screen.height}");
			UpdateCanvasRect();
		}

		private void Update ()
		{
			UpdateCanvasRect();
			if (Application.isPlaying && !Application.isEditor)
				Destroy(this);
		}

		private void UpdateCanvasRect ()
		{
			
			RectTransform rectTransform = (RectTransform)worldCanvas.transform;
			float measuredAspectRatio = (float)Screen.width / Screen.height;
			rectTransform.sizeDelta = new Vector2(Mathf.Min(rectTransform.sizeDelta.y * measuredAspectRatio, rectTransform.sizeDelta.y * maxAspectRatio.x / maxAspectRatio.y), rectTransform.sizeDelta.y);
		}
	}
}
