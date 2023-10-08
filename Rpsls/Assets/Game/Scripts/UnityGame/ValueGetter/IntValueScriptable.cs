using UnityEngine;

namespace Kalkatos.UnityGame
{
	[CreateAssetMenu(fileName = "NewIntScriptable", menuName = "Value Scriptable/Int")]
	public class IntValueScriptable : ScriptableObject, IValueGetter<int>
	{
		[SerializeField] private IntValueGetter intValue;

		public int GetValue () => intValue.GetValue();
	}
}
