using System;
using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewScreenSignal", menuName = "Signals/Signal (Screen)", order = 4)]
	public class ScreenSignal : TypedSignal<bool>
	{
		public bool DefaultState;

		public override void Emit ()
		{
			base.Emit();
			base.EmitWithParam(DefaultState);
			Logger.Log("Emitted signals for Screen " + name);
		}

		public override void EmitWithParam (bool param)
		{
			base.Emit();
			base.EmitWithParam(param);
			Logger.Log("Emitted signals for Screen " + name);
		}
	}
}