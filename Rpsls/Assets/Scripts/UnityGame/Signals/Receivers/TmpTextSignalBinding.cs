using TMPro;
using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	public class TmpTextSignalBinding : MonoBehaviour
	{
		[SerializeField] private SignalString signal;
		[SerializeField] private TMP_Text textComponent;

		private void Reset ()
		{
			if (textComponent == null)
				textComponent = GetComponent<TMP_Text>();
		}

		private void OnEnable ()
		{
			signal.OnSignalEmittedWithParam.AddListener(UpdateText);
			UpdateText(signal.LastValue);
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