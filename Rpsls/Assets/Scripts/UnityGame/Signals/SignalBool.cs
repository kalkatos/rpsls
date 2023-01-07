using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignalBool", menuName = "Signal/Signal (Bool)", order = 3)]
	public class SignalBool : TypedSignal<bool>
	{
		public bool Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}