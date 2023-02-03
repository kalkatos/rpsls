﻿using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewSignalFloat", menuName = "Signals/Signal (Float)", order = 6)]
	public class SignalFloat : TypedSignal<float>
	{
		public int Param;

		public override void Emit ()
		{
			EmitWithParam(Param);
		}
	}
}