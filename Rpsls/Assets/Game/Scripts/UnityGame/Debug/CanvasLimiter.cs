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
			UpdateCanvasRect();
		}

		private void OnRectTransformDimensionsChange ()
		{
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
