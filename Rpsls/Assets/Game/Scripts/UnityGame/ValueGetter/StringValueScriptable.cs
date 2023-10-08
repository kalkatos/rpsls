using UnityEngine;

namespace Kalkatos.UnityGame
{
	[CreateAssetMenu(fileName = "NewStringScriptable", menuName = "Value Scriptable/String")]
	public class StringValueScriptable : ScriptableObject, IValueGetter<string>
	{
		[SerializeField] private StringValueGetter stringValue;

		public string GetValue () => stringValue.GetValue();
	}
}
