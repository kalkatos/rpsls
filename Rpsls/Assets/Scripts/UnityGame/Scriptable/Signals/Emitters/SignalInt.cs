using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalInt", menuName = "Signals/Signal (Int)", order = 5)]
	public class SignalInt : TypedSignal<int>
	{
		public int Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}