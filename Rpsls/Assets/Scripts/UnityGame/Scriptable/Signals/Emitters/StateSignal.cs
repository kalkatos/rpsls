using UnityEngine;
using Sirenix.OdinInspector;

namespace Kalkatos.UnityGame.Scriptable.Network
{
	[CreateAssetMenu(fileName = "NewStateSignal", menuName = "Signals/Signal (State)", order = 4)]
	public class StateSignal : TypedSignal<string>
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
		}

		public void EmitWithParam (float value)
		{
			Value = value.ToString();
		}

		public void EmitWithParam (bool value)
		{
			Value = value ? "1" : "0";
		}
	}
}