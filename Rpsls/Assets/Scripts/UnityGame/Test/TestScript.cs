using UnityEngine;
using Newtonsoft.Json;
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
			string arraystr = "";
			string[] arr = arraystr.Split("|");
			UnityEngine.Debug.Log(arr + " _ " + JsonConvert.SerializeObject(arr));
		}
#endif
	}
}