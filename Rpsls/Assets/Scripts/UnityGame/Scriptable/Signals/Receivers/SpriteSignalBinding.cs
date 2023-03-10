using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.UnityGame.Scriptable
{
	public class SpriteSignalBinding : MonoBehaviour
	{
		[SerializeField, LabelText("Signal"), HorizontalGroup("Signal", 30)] private SignalType signalType;
		[SerializeField, HideLabel, HorizontalGroup("Signal"), ShowIf(nameof(signalType), SignalType.Int)] private TypedSignal<int> signalByIndex;
		[SerializeField, HideLabel, HorizontalGroup("Signal"), ShowIf(nameof(signalType), SignalType.String)] private TypedSignal<string> signalByString;
		[SerializeField, LabelText("Renderer"), HorizontalGroup("Renderer", 30)] private VisualType visualType;
		[SerializeField, HideLabel, HorizontalGroup("Renderer"), ShowIf(nameof(visualType), VisualType.Sprite)] private SpriteRenderer spriteRenderer;
		[SerializeField, HideLabel, HorizontalGroup("Renderer"), ShowIf(nameof(visualType), VisualType.UI)] private Image uiImage;
		[SerializeField] private SpriteListScriptable sprites;

		public enum SignalType { Int, String }
		public enum VisualType { UI, Sprite }

		private void Reset ()
		{
			if (spriteRenderer == null && TryGetComponent(out spriteRenderer))
				visualType = VisualType.Sprite;
			if (uiImage == null && TryGetComponent(out uiImage))
				visualType = VisualType.UI;
		}

		private void OnEnable ()
		{
			if (signalByIndex != null)
			{
				signalByIndex.OnSignalEmittedWithParam.AddListener(UpdateSpriteByIndex);
				UpdateSpriteByIndex(signalByIndex.Value);
			}
			if (signalByString != null)
			{
				signalByString.OnSignalEmittedWithParam.AddListener(UpdateSpriteByName);
				UpdateSpriteByName(signalByString.Value);
			}
		}

		private void OnDisable ()
		{
			if (signalByIndex != null)
			{
				signalByIndex.OnSignalEmittedWithParam.AddListener(UpdateSpriteByIndex);
				UpdateSpriteByIndex(signalByIndex.Value);
			}
			if (signalByString != null)
			{
				signalByString.OnSignalEmittedWithParam.AddListener(UpdateSpriteByName);
				UpdateSpriteByName(signalByString.Value);
			}
		}

		private void UpdateSpriteByIndex (int index)
		{
			Sprite sprite = sprites.GetByIndex(index);
			if (spriteRenderer != null)
				spriteRenderer.sprite = sprite;
			if (uiImage != null)
				uiImage.sprite = sprite;
		}

		private void UpdateSpriteByName (string name)
		{
			Sprite sprite = sprites.GetByName(name);
			if (spriteRenderer != null)
				spriteRenderer.sprite = sprite;
			if (uiImage != null)
				uiImage.sprite = sprite; ;
		}
	}
}     