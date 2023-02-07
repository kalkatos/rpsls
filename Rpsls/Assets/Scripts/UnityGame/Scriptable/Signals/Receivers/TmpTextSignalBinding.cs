using TMPro;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable
{
	public class TmpTextSignalBinding : MonoBehaviour
	{
		[SerializeField] private TypedSignal<string> signal;
		[SerializeField] private TMP_Text textComponent;

		private void Reset ()
		{
			if (textComponent == null)
				textComponent = GetComponent<TMP_Text>();
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
			textComponent.text = text;
		}
	}
}