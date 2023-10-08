using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable
{
	public class TmpTextSignalBinding : MonoBehaviour
	{
		[SerializeField] private TypedSignal<string> signal;
		[SerializeField, HideIf(nameof(textComponentInputField))] private TMP_Text textComponent;
		[SerializeField, HideIf(nameof(textComponent))] private TMP_InputField textComponentInputField;

		private void Reset ()
		{
			if (textComponent == null)
				textComponent = GetComponent<TMP_Text>();
			if (textComponentInputField == null)
				textComponentInputField = GetComponent<TMP_InputField>();
		}

		private void OnEnable ()
		{
			signal.OnSignalEmittedWithParam.AddListener(UpdateText);
			UpdateText(signal.Value);
		}

		private void OnDisable ()
		{
			signal.OnSignalEmittedWithParam.RemoveListener(UpdateText);
		}

		private void UpdateText (string text)
		{
			textComponent?.SetText(text);
			textComponentInputField?.SetTextWithoutNotify(text);
		}
	}
}