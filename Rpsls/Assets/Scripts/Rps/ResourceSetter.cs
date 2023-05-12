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
			byte[] bytes = File.ReadAllBytes($"{Application.dataPath}/uris.cfg");
			string config = Encoding.UTF8.GetString(bytes);
			Storage.Save("Uris", config);
		}
	}
}