using UnityEngine;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestScript : MonoBehaviour
{
#if UNITY_EDITOR
	[MenuItem("Test/Test")]
    public static void Test ()
    {
		Debug.Log(JsonConvert.SerializeObject(new Foo { Index = 1 }));
	}

	public class Foo
	{
		public int Index;
		public int Hash;

		public Foo ()
		{
			UpdateHash();
		}

		public void UpdateHash ()
		{
			Hash = Index + 3;
		}
	}

#endif
}
