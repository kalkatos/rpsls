using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalState", menuName = "Signals/Signal (State)", order = 4)]
	public class SignalState : TypedSignal<string>
	{
		[PropertyOrder(0)]
		public string Key;

		public override void Emit ()
		{
			EmitWithParam(Value);
		}

		public void EmitWithParam (int value)
		{
			Value = value.ToString();
			Emit();
		}

		public void EmitWithParam (float value)
		{
			Value = value.ToString();
			Emit();
		}

		public void EmitWithParam (bool value)
		{
			Value = value ? "1" : "0";
			Emit();
		}
	}
}