using UnityEngine;
using TMPro;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Signals
{
	public class TmpInputFieldSignalBinding : MonoBehaviour
	{
		[SerializeField] private SignalString signal;
		[SerializeField] private TMP_InputField inputFieldComponent;

		private void Reset ()
		{
			if (inputFieldComponent == null)
				inputFieldComponent = GetComponent<TMP_InputField>();
		}

		private void OnEnable ()
		{
			signal.OnSignalEmittedWithParam.AddListener(UpdateText);
			inputFieldComponent.onEndEdit.AddListener(Submit);
			UpdateText(signal.LastValue);
		}

		private void OnDisable ()
		{
			signal.OnSignalEmittedWithParam.RemoveListener(UpdateText);
			inputFieldComponent.onEndEdit.RemoveListener(Submit);
		}

		private void UpdateText (string text)
		{
			inputFieldComponent.SetTextWithoutNotify(text);
		}

		private void Submit (string text)
		{
			signal.EmitWithParam(text);
			NetworkClient.SetNickname(text);
		}
	}
}