using UnityEngine;

namespace Kalkatos.UnityGame.Signals
{
	[CreateAssetMenu(fileName = "NewScreenSignal", menuName = "Signals/Signal (Screen)", order = 4)]
	public class ScreenSignal : TypedSignal<bool>
	{
		public bool DefaultState;

		public override void Emit ()
		{
			EmitWithParam(DefaultState);
		}
	}
}