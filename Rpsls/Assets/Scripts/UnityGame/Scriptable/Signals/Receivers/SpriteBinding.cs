using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kalkatos.UnityGame.Scriptable
{
	public class SpriteBinding : MonoBehaviour
	{
#pragma warning disable
		[SerializeField, LabelText("Value"), HorizontalGroup("Value", 30)] private ValueType valueType;
		[SerializeField, HideLabel, HorizontalGroup("Value"), ShowIf(nameof(valueType), ValueType.Int)] private IntValueGetter indexValue;
		[SerializeField, HideLabel, HorizontalGroup("Value"), ShowIf(nameof(valueType), ValueType.String)] private StringValueGetter nameValue;
		[SerializeField, LabelText("Renderer"), HorizontalGroup("Renderer", 30)] private VisualType visualType;
		[SerializeField, HideLabel, HorizontalGroup("Renderer"), ShowIf(nameof(visualType), VisualType.Sprite)] private SpriteRenderer spriteRenderer;
		[SerializeField, HideLabel, HorizontalGroup("Renderer"), ShowIf(nameof(visualType), VisualType.UI)] private Image uiImage;
		[SerializeField] private SpriteListScriptable sprites;
#pragma warning restore

		public enum ValueType { Int, String }
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
			switch (valueType)
			{
				case ValueType.Int:
					if (indexValue != null)
					{
						if (indexValue.Type == IntValueGetter.ValueType.Scriptable && (indexValue.ScriptableValue is TypedSignal<int>))
							((TypedSignal<int>)indexValue.ScriptableValue).OnSignalEmittedWithParam.AddListener(UpdateSpriteByIndex);
						UpdateSpriteByIndex(indexValue.GetValue());
					}
					break;
				case ValueType.String:
					if (nameValue != null)
					{
						if (nameValue.Type == StringValueGetter.ValueType.Scriptable && (nameValue.ScriptableValue is TypedSignal<string>))
							((TypedSignal<string>)nameValue.ScriptableValue).OnSignalEmittedWithParam.AddListener(UpdateSpriteByName);
						UpdateSpriteByName(nameValue.GetValue());
					}
					break;
			}
		}

		private void OnDisable ()
		{
			switch (valueType)
			{
				case ValueType.Int:
					if (indexValue != null)
					{
						if (indexValue.Type == IntValueGetter.ValueType.Scriptable && (indexValue.ScriptableValue is TypedSignal<int>))
							((TypedSignal<int>)indexValue.ScriptableValue).OnSignalEmittedWithParam.RemoveListener(UpdateSpriteByIndex);
						UpdateSpriteByIndex(indexValue.GetValue());
					}
					break;
				case ValueType.String:
					if (nameValue != null)
					{
						if (nameValue.Type == StringValueGetter.ValueType.Scriptable && (nameValue.ScriptableValue is TypedSignal<string>))
							((TypedSignal<string>)nameValue.ScriptableValue).OnSignalEmittedWithParam.RemoveListener(UpdateSpriteByName);
						UpdateSpriteByName(nameValue.GetValue());
					}
					break;
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
				uiImage.sprite = sprite;
		}
	}
}