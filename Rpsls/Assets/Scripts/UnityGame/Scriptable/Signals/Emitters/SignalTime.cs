using System;
using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "NewSignalTime", menuName = "Signals/Signal (Time)", order = 7)]
	public class SignalTime : TypedSignal<string>
	{
		public override void Emit ()
		{
			base.EmitWithParam(DateTime.UtcNow.ToString("u"));
		}

		public override void EmitWithParam (string param)
		{
			base.EmitWithParam(DateTime.UtcNow.ToString("u"));
		}
	}
}