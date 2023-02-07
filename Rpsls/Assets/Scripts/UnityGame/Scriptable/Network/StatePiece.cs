using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kalkatos.UnityGame.Scriptable.Network
{

	[CreateAssetMenu(fileName = "NewStatePiece", menuName = "Network/State Piece")]
	public class StatePiece : ScriptableObject
	{
		[SerializeField] private string key;
		[SerializeField] private string value;
		[SerializeField] private UnityEvent<string> onKeySet;
		[SerializeField] private UnityEvent<string> onValueSet;
		public List<StateCheck> Checks;

		public string Key => key;
		public string Value => value;

		public void SetKey (string key)
		{
			this.key = key;
			onKeySet?.Invoke(key);
		}

		public void SetValue (string value)
		{
			this.value = value;
			onValueSet?.Invoke(value);
			foreach (var item in Checks)
				item.Check(value);
		}
	}

	[Serializable]
	public class StateCheck
	{
		public string ExpectedValue;
		public UnityEvent OnCheckValid;

		public void Check (string value)
		{
			if (value == ExpectedValue)
				OnCheckValid?.Invoke();
		}
	}
}