using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Kalkatos.UnityGame
{
	[Serializable, InlineProperty]
	public class FloatValueGetter : IValueGetter<float>
	{
		[HorizontalGroup(0.3f), HideLabel]
		public ValueType Type;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Simple)]
		public float SimpleValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Random)]
		public Vector2 RandomValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Scriptable)]
		public ScriptableObject ScriptableValue;

		public enum ValueType { Simple, Random, Scriptable }

		public float GetValue ()
		{
			switch (Type)
			{
				case ValueType.Simple:
					return SimpleValue;
				case ValueType.Random:
					return UnityEngine.Random.Range(RandomValue.x, RandomValue.y);
				case ValueType.Scriptable:
					if (ScriptableValue != null)
					{
						if (ScriptableValue is IValueGetter<float>)
							return ((IValueGetter<float>)ScriptableValue).GetValue();
						Logger.LogWarning($"[FloatValueGetter] The scriptable {ScriptableValue.name} is not a IValueGetter.");
					}
					else
						Logger.LogWarning($"[FloatValueGetter] The scriptable value is null.");
					break;
			}
			return default;
		}
	}

	[Serializable, InlineProperty]
	public class IntValueGetter : IValueGetter<int>
	{
		[HorizontalGroup(0.3f), HideLabel]
		public ValueType Type;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Simple)]
		public int SimpleValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Random)]
		public Vector2Int RandomValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Scriptable)]
		public ScriptableObject ScriptableValue;

		public enum ValueType { Simple, Random, Scriptable }

		public int GetValue ()
		{
			switch (Type)
			{
				case ValueType.Simple:
					return SimpleValue;
				case ValueType.Random:
					return UnityEngine.Random.Range(RandomValue.x, RandomValue.y);
				case ValueType.Scriptable:
					if (ScriptableValue != null)
					{
						if (ScriptableValue is IValueGetter<int>)
							return ((IValueGetter<int>)ScriptableValue).GetValue();
						Logger.LogWarning($"[IntValueGetter] The scriptable {ScriptableValue.name} is not a IValueGetter.");
					}
					else
						Logger.LogWarning($"[IntValueGetter] The scriptable value is null.");
					break;
			}
			return default;
		}
	}

	[Serializable, InlineProperty]
	public class StringValueGetter : IValueGetter<string>
	{
		[HorizontalGroup(0.3f), HideLabel]
		public ValueType Type;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Simple)]
		public string SimpleValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Scriptable)]
		public ScriptableObject ScriptableValue;

		public enum ValueType { Simple, Scriptable }

		public string GetValue ()
		{
			switch (Type)
			{
				case ValueType.Simple:
					return SimpleValue;
				case ValueType.Scriptable:
					if (ScriptableValue != null)
					{
						if (ScriptableValue is IValueGetter<string>)
							return ((IValueGetter<string>)ScriptableValue).GetValue();
						Logger.LogWarning($"[StringValueGetter] The scriptable {ScriptableValue.name} is not a IValueGetter.");
					}
					else
						Logger.LogWarning($"[StringValueGetter] The scriptable value is null.");
					break;
			}
			return default;
		}
	}

	[Serializable, InlineProperty]
	public class BoolValueGetter : IValueGetter<bool>
	{
		[HorizontalGroup(0.3f), HideLabel]
		public ValueType Type;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Simple)]
		public bool SimpleValue;
		[HorizontalGroup(0.7f), HideLabel, ShowIf(nameof(Type), ValueType.Scriptable)]
		public ScriptableObject ScriptableValue;

		public enum ValueType { Simple, Scriptable }

		public bool GetValue ()
		{
			switch (Type)
			{
				case ValueType.Simple:
					return SimpleValue;
				case ValueType.Scriptable:
					if (ScriptableValue != null)
					{
						if (ScriptableValue is IValueGetter<bool>)
							return ((IValueGetter<bool>)ScriptableValue).GetValue();
						Logger.LogWarning($"[BoolValueGetter] The scriptable {ScriptableValue.name} is not a IValueGetter.");
					}
					else
						Logger.LogWarning($"[BoolValueGetter] The scriptable value is null.");
					break;
			}
			return default;
		}
	}
}
