using System.Linq;
using UnityEditor;
using Kalkatos.UnityGame.Screens;
using Kalkatos.UnityGame.Signals;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Kalkatos.UnityGame.Editor
{
	[CustomEditor(typeof(ScreenManager))]
    public class ScreenManagerInspector : OdinEditor
    {
		private void GetScreenSignals ()
		{
			ScreenManager manager = (ScreenManager)target;
			ScreenSignal[] screenSignals = AssetDatabase.FindAssets("t:" + nameof(ScreenSignal))
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.Select(x => AssetDatabase.LoadAssetAtPath<ScreenSignal>(x)).ToArray();
			var diff = screenSignals.Except(manager.ScreenSignals);
			if (diff != null && diff.Count() > 0)
			{
				manager.ScreenSignals = screenSignals;
				EditorUtility.SetDirty(manager);
			}
		}

		protected override void OnEnable ()
		{
			base.OnEnable();
			GetScreenSignals();
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Force Get Screen Signals"))
				GetScreenSignals();
		}
	}
}