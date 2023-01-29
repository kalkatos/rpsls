using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewScreenSignal", menuName = "Signals/Signal (Screen)", order = 4)]
	public class ScreenSignal : TypedSignal<bool>
	{
		public bool IsScene;

		public override void Emit ()
		{
			base.Emit();
			base.EmitWithParam(true);
			if (IsScene)
				SceneManager.LoadScene(name);
		}

		public override void EmitWithParam (bool param)
		{
			base.Emit();
			base.EmitWithParam(param);
			if (param && IsScene)
				SceneManager.LoadScene(name);
		}
	}
}