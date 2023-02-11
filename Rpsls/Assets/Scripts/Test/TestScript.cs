using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestScript : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Test/Test")]
    public static void Test ()
    {
		Dictionary<string, string> dict = new Dictionary<string, string> ();
		dict["Key1"] = "Value1";
		Debug.Log(JsonConvert.SerializeObject(dict));
	}

#endif
}
