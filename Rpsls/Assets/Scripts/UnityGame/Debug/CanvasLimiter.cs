using UnityEngine;

namespace Kalkatos.UnityGame
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
			if (Application.isPlaying && !Application.isEditor)
				Destroy(this);
		}
	}
}
