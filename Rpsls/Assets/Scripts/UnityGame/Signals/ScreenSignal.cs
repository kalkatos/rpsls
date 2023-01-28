using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewScreenSignal", menuName = "Signals/Signal (Screen)", order = 4)]
	public class ScreenSignal : TypedSignal<bool>
	{
		public override void Emit ()
		{
			base.Emit();
			base.EmitWithParam(true);
			try { SceneManager.LoadScene(name); }
			catch (Exception) { }
		}

		public override void EmitWithParam (bool param)
		{
			base.Emit();
			base.EmitWithParam(param);
			if (true)
				try { SceneManager.LoadScene(name); }
				catch (Exception) { }
		}
	}
}