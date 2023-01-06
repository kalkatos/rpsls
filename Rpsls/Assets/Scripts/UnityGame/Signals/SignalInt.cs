using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignalInt", menuName = "Signal/Signal (Int)", order = 2)]
	public class SignalInt : TypedSignal<int>
	{
		public int Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}