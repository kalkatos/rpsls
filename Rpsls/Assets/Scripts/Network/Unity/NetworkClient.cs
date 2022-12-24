using Kalkatos.Network.Specific;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Kalkatos.Network.Unity
{
	public class NetworkClient : MonoBehaviour
	{
		public static NetworkClient Instance;

		private INetworkClient _networkClient = new AzureFunctionsNetworkClient();

		private void Awake ()
		{
			Instance = this;
		}

		public static void Connect (object parameters, Action<object> onSuccess, Action<object> onFailure)
		{
			Instance._networkClient.Connect(parameters, onSuccess, onFailure);
		}
	}
}