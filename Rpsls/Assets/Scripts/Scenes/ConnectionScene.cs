using UnityEngine;
using Kalkatos.Network.Model;
using Kalkatos.Network.Unity;

namespace Kalkatos.UnityGame.Systems
{
    public class ConnectionScene : MonoBehaviour
    {
		[Header("References - UI")]
		[SerializeField] private GameObject _notConnectedMessage;

		private void Awake ()
		{
			_notConnectedMessage.SetActive(false);
		}

		private void Start ()
        {
			TryConnection();
        }

        private void TryConnection ()
        {
			NetworkClient.Connect(null,
				(success) =>
				{
					SceneManager.EndScene("Connection", "ToMenu");
				},
				(failure) =>
				{
					if (failure is NetworkError && (NetworkError)failure == NetworkError.NotConnected)
						_notConnectedMessage.SetActive(true);
					else
						SceneManager.EndScene("Connection", "ToLogin");
				});
		}
    }
}
