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
			byte[] bytes = File.ReadAllBytes($"{Application.dataPath}/uris");
			Storage.Save("Uris", Encoding.UTF8.GetString(bytes));
		}
	}
}