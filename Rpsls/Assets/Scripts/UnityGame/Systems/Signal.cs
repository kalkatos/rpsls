using System;
using UnityEngine;

namespace Kalkatos.UnityGame.Systems
{
	[CreateAssetMenu(fileName = "NewSignal", menuName = "Signal")]
	public class Signal : ScriptableObject
	{
		public event Action OnSignalEmitted;

		public void Emit ()
		{
			OnSignalEmitted?.Invoke();
		}
	}
}