using UnityEngine;
using UnityEditor;
using Kalkatos.UnityGame.Screens;

namespace Kalkatos.UnityGame.Editor
{
	[CustomPropertyDrawer(typeof(ScreenPointer))]
	public class ScreenPointerDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			var checkboxWidth = 15;
			var checkboxRect = new Rect(position.x + position.width - checkboxWidth, position.y, checkboxWidth, position.height);
			var controlRect = new Rect(position.x, position.y, position.width - checkboxWidth, position.height);
			var boolProp = property.FindPropertyRelative(nameof(ScreenPointer.IsScene));
			if (boolProp.boolValue)
				EditorGUI.PropertyField(controlRect, property.FindPropertyRelative(nameof(ScreenPointer.SceneName)), label);
			else
			{
				if (label == GUIContent.none)
					label = new GUIContent(nameof(ScreenPointer.ScreenName));
				EditorGUI.PropertyField(controlRect, property.FindPropertyRelative(nameof(ScreenPointer.ScreenName)), label);
			}
			EditorGUI.PropertyField(checkboxRect, boolProp, GUIContent.none);
		}
	}
}
