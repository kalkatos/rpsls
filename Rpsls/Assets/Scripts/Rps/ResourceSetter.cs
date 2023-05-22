using System.IO;
using System.Text;
using UnityEngine;

namespace Kalkatos.UnityGame.Rps
{
	[DefaultExecutionOrder(-999)]
    public class ResourceSetter : MonoBehaviour
    {
		private void Awake ()
		{
			byte[] bytes = File.ReadAllBytes($"{Application.dataPath}/urlprefix.cfg");
			if (bytes == null)
				return;
			string config = Encoding.UTF8.GetString(bytes);
			if (string.IsNullOrEmpty(config))
				return;
			Storage.Save("UrlPrefix", config);
		}
	}
}