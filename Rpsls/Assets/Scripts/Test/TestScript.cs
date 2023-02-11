using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestScript : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Test/Test")]
    public static void Test ()
    {
		string value = "1940-13";
		if (DateTime.TryParse(value, out DateTime time))
			Debug.Log("It's a DateTime, and this is it parsed: " + time.ToUniversalTime());
		else if (float.TryParse(value, out float fValue))
			Debug.Log("It's a float, and this is it parsed: " + fValue);
		else
			Debug.Log("It's nothing of those");
	}
#endif
}
