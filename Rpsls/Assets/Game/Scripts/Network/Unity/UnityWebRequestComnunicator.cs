#if UNITY_2018_1_OR_NEWER

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Kalkatos.Network.Unity
{
	public class UnityWebRequestComnunicator : ICommunicator
	{
		private MonoBehaviour mono;

		public UnityWebRequestComnunicator (MonoBehaviour mono)
		{
			this.mono = mono;
		}

		public void Get (string url, Action<string> callback)
		{
			mono.StartCoroutine(GetCoroutine(url, callback));
		}

		public void Post (string url, string message, Action<string> callback)
		{
			mono.StartCoroutine(PostCoroutine(url, message, callback));
		}

		private IEnumerator GetCoroutine (string url, Action<string> callback) 
		{
			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.downloadHandler = new DownloadHandlerBuffer();
				UnityWebRequestAsyncOperation operation = request.SendWebRequest();
				yield return operation;
				string result = request.downloadHandler.text;
				callback?.Invoke(result);
			}
		}

		private IEnumerator PostCoroutine (string url, string message, Action<string> callback)
		{
			using (UnityWebRequest request = new UnityWebRequest(url))
			{
				request.method = "POST";
				request.SetRequestHeader("Content-Type", "application/json");
				request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(message));
				request.uploadHandler.contentType = "application/json";
				request.downloadHandler = new DownloadHandlerBuffer();
				TaskCompletionSource<bool> taskAwaiter = new TaskCompletionSource<bool>();
				UnityWebRequestAsyncOperation operation = request.SendWebRequest();
				yield return operation;
				string result = request.downloadHandler.text;
				callback?.Invoke(result);
			}
		}
	}
}

#endif