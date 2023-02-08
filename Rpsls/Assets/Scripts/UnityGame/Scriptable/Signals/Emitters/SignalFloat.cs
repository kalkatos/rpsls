using UnityEngine;
using UnityEngine.Serialization;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalFloat", menuName = "Signals/Signal (Float)", order = 6)]
	public class SignalFloat : TypedSignal<float>
	{
		[FormerlySerializedAs("Param")] public int DefaultValue;

		public override void Emit ()
		{
			EmitWithParam(DefaultValue);
		}
	}
}