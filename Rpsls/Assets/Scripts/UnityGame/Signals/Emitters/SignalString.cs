using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignalString", menuName = "Signals/Signal (String)", order = 1)]
	public class SignalString : TypedSignal<string>
	{
		public string Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}