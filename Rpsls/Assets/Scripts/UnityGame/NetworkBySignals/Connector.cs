using UnityEngine;
using Kalkatos.Network.Unity;
using Kalkatos.Network.Model;
using Kalkatos.UnityGame.Signals;

namespace Kalkatos.UnityGame.NetworkBySignals
{
	[CreateAssetMenu(fileName = "Connector", menuName = "Network/Connector", order = 0)]
	public class Connector : ScriptableObject
    {
		[SerializeField] private Signal onConnected;
		[SerializeField] private Signal onNotConnectedError;

        public void TryConnection ()
        {
			if (NetworkClient.IsConnected)
			{
				Logger.LogWarning("Already connected.");
				return;
			}

			Logger.Log("Trying connection...");
			NetworkClient.Connect(
				(success) =>
				{
					onConnected?.Emit();
					Storage.Save("IsNewUser", success ? 1 : 0);
					Logger.Log("Connected Successfully!");
				},
				(failure) =>
				{
					if (failure.Tag == NetworkErrorTag.NotConnected)
						onNotConnectedError?.Emit();
					Logger.Log("Connection Error: " + failure.Message);
				});
		}
    }
}
