using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
    public class RuntimeStartRunner : MonoBehaviour
    {
		[RuntimeInitializeOnLoadMethod]
		public static void DoRuntimeStartRoutines ()
		{
			LoadUrlPrefix();
		}

		private static void LoadUrlPrefix ()
		{
			Logger.Log("Runtime Start Runner");
			TextAsset configAsset = (TextAsset)Resources.Load("urlprefix");
			if (configAsset == null)
			{
				Logger.LogWarning("UrlPrefix asset not found.");
				return; 
			}
			if (string.IsNullOrEmpty(configAsset.text))
			{
				Logger.LogWarning("UrlPrefix asset text is empty.");
				return; 
			}
			Storage.Save("UrlPrefix", configAsset.text);
		}
	}
}