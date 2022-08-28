using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Kalkatos.Rpsls
{
    [RequireComponent(typeof(PhotonView))]
    public class PhotonNetworkBehaviour : MonoBehaviourPunCallbacks
    {
        private PhotonNetworkManager manager;

        internal void SetManager (PhotonNetworkManager manager)
        {
            this.manager = manager;
        }

        public override void OnConnectedToMaster ()
        {
            manager.OnConnectedToMaster();
        }

        public override void OnJoinedLobby ()
        {
            manager.RaiseLogInSuccess();
        }

        public override void OnPlayerEnteredRoom (Player newPlayer)
        {
            //manager.RaisePlayerEnteredLobby(PhotonNetworkManager.InfoFromPlayer(newPlayer));
        }

        public override void OnPlayerLeftRoom (Player otherPlayer)
        {
            //manager.RaisePlayerLeftLobby(PhotonNetworkManager.InfoFromPlayer(otherPlayer));
        }

        public override void OnJoinedRoom ()
        {
            manager.OnJoinedRoom();
        }

        public override void OnLeftRoom ()
        {
            manager.OnLeftRoom();
        }

        public override void OnMasterClientSwitched (Player newMasterClient)
        {
            manager.OnMasterClientSwitched(newMasterClient);
        }

        public override void OnPlayerPropertiesUpdate (Player targetPlayer, Hashtable changedProps)
        {
            manager.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        }
    }
}
