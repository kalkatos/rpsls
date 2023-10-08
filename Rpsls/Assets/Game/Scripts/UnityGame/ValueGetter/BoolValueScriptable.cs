using UnityEngine;

namespace Kalkatos.UnityGame
{
	[CreateAssetMenu(fileName = "NewBoolScriptable", menuName = "Value Scriptable/Bool")]
	public class BoolValueScriptable : ScriptableObject, IValueGetter<bool>
	{
		[SerializeField] private BoolValueGetter boolValue;

		public bool GetValue () => boolValue.GetValue();
	}
}
