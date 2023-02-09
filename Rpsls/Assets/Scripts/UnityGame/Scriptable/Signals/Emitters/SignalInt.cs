using UnityEngine;
using UnityEngine.Serialization;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalInt", menuName = "Signals/Signal (Int)", order = 5)]
	public class SignalInt : TypedSignal<int>
	{
		public int DefaultValue;

		public override void Emit ()
		{
			EmitWithParam(DefaultValue);
		}
	}
}