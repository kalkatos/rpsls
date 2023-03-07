using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kalkatos.UnityGame.Test
{
	public class TestScript : MonoBehaviour
	{
#if UNITY_EDITOR
		[MenuItem("Test/Test")]
		public static void Test ()
		{
			string value = "1940-13";
			if (DateTime.TryParse(value, out DateTime time))
				UnityEngine.Debug.Log("It's a DateTime, and this is it parsed: " + time.ToUniversalTime());
			else if (float.TryParse(value, out float fValue))
				UnityEngine.Debug.Log("It's a float, and this is it parsed: " + fValue);
			else
				UnityEngine.Debug.Log("It's nothing of those");
		}
#endif
	}
}