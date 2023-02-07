using UnityEngine;

namespace Kalkatos.UnityGame.Scriptable
{
	[CreateAssetMenu(fileName = "SignalsSettings", menuName = "Signals/SignalsSettings", order = 0)]
	public class SignalsSettings : SingletonScriptableObject<SignalsSettings>
	{
		public bool EmitDebug;

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Tools/Signals/Debug ON")]
		public static void TurnOnSignalDebug ()
		{
			Instance.EmitDebug = true;
		}

		[UnityEditor.MenuItem("Tools/Signals/Debug OFF")]
		public static void TurnOffSignalDebug ()
		{
			Instance.EmitDebug = false; 
		}
#endif
	}
}