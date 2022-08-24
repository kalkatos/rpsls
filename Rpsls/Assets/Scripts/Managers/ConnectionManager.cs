using Photon.Pun;

namespace Kalkatos.Rpsls
{
    public class ConnectionManager : MonoBehaviourPunCallbacks
    {
        private void Start ()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster ()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby ()
        {
            SceneManager.EndScene();
        }
    }
}
