using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
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
			UnityEngine.Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, string> { { "MustRunLocally", "0" }, { "MustRunLocally2", "1" } }));
		}
#endif
	}
}