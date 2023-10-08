using UnityEngine;
using UnityEngine.Serialization;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalBool", menuName = "Signals/Signal (Bool)", order = 1)]
	public class SignalBool : TypedSignal<bool>
	{
		public bool DefaultValue;

		public override void Emit ()
		{
			EmitWithParam(DefaultValue);
		}
	}
}