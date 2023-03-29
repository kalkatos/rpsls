using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kalkatos.UnityGame.Editor
{
    public class MiroUnity : EditorWindow
    {

		Vector2 scrollPosition;

		[MenuItem("Tools/Open Miro Window")]
		static void OpenWindow ()
		{
			GetWindow<MiroUnity>();
		}

		void OnGUI ()
		{
			// Begin scrollable area
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			// Display a button to open the Scene view window
			if (GUILayout.Button("Open Scene View"))
			{
				EditorWindow.GetWindow<SceneView>();
			}

			// Display a button to open the Hierarchy window
			if (GUILayout.Button("Open Hierarchy"))
			{
				EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow"));
			}

			// End scrollable area
			EditorGUILayout.EndScrollView();
		}
	}
}