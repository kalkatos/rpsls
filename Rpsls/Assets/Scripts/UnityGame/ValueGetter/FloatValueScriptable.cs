using UnityEngine;

namespace Kalkatos.UnityGame
{
	[CreateAssetMenu(fileName = "NewFloatScriptable", menuName = "Value Scriptable/Float")]
	public class FloatValueScriptable : ScriptableObject, IValueGetter<float>
	{
		[SerializeField] private FloatValueGetter floatValue;

		public float GetValue () => floatValue.GetValue();
	}
}
