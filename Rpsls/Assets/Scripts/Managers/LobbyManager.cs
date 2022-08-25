using System;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

namespace Kalkatos.Rpsls
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public static event Action OnFailedToJoinRoom;
        public static event Action OnNotConnected;

        private void Start ()
        {
            if (!PhotonNetwork.IsConnected)
            {
                OnNotConnected?.Invoke();
                Debug.LogWarning("Not connected.");
            }
        }

        private static string GetRandomRoomName ()
        {
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            for (int i = 0; i < 6; i++)
                result += letters[Random.Range(0, letters.Length)];
            return result;
        }

        public override void OnJoinedRoom ()
        {
            SceneManager.EndScene();
        }

        public static void TryJoiningRoom (string roomId)
        {
            if (!PhotonNetwork.JoinRoom(roomId))
                OnFailedToJoinRoom?.Invoke();
        }

        public static void CreateRoom ()
        {
            PhotonNetwork.CreateRoom(GetRandomRoomName());
        }

        public static void SetPlayerNickName (string nickname)
        {
            PhotonNetwork.LocalPlayer.NickName = nickname;
        }
    }
}
