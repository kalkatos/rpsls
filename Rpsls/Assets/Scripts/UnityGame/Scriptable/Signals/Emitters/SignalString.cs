using UnityEngine;
using UnityEngine.Serialization;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalString", menuName = "Signals/Signal (String)", order = 2)]
	public class SignalString : TypedSignal<string>
	{
		public string Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}