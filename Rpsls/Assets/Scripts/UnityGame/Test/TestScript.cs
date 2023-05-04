using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kalkatos.UnityGame.Test
{
	public class TestScript : MonoBehaviour
	{
#if UNITY_EDITOR
		private static TestScript instance;

		[SerializeField] private string url;
		[SerializeField] private string message;

		private void Awake ()
		{
			instance = this;
		}

		[MenuItem("Test/Test")]
		public static void Test ()
		{
			UnityEngine.Debug.Log("Requesting");
			UnityWebRequest www = new UnityWebRequest(instance.url);
			www.method = "POST";
			www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(instance.message));
			www.uploadHandler.contentType = "application/json";
			www.downloadHandler = new DownloadHandlerBuffer();
			instance.StartCoroutine(Wait(www.SendWebRequest()));
		}

		private static IEnumerator Wait (UnityWebRequestAsyncOperation operation)
		{
			WaitUntil waitUntil = new WaitUntil(() => operation.isDone);
			yield return waitUntil;
			UnityEngine.Debug.Log($"Operation : {JsonConvert.SerializeObject(operation)}");
			UnityEngine.Debug.Log($"WebRequest : {JsonConvert.SerializeObject(operation?.webRequest)}");
			UnityEngine.Debug.Log($"DownloadHandler : {JsonConvert.SerializeObject(operation?.webRequest?.downloadHandler)}");
			UnityEngine.Debug.Log($"Finished : {operation?.webRequest?.downloadHandler?.text}");
			operation.webRequest.Dispose();
		}
#endif
	}
}