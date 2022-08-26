using System;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;
using Photon.Realtime;

namespace Kalkatos.Rpsls
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public static LobbyManager Instance { get; private set; }

        public static event Action OnFailedToJoinRoom;
        public static event Action OnNotConnected;

        private Settings settings;

        private void Awake ()
        {
            Instance = this;
            settings = Resources.Load<Settings>("RpslsGameSettings");
        }

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

        public override void OnJoinRoomFailed (short returnCode, string message)
        {
            OnFailedToJoinRoom?.Invoke();
        }

        public static void TryJoiningRoom (string roomId)
        {
            if (!PhotonNetwork.JoinRoom(roomId))
                OnFailedToJoinRoom?.Invoke();
        }

        public static void CreateRoom ()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = (byte)Instance.settings.MaxPlayers;
            roomOptions.PublishUserId = true;
            PhotonNetwork.CreateRoom(GetRandomRoomName(), roomOptions);
        }

        public static void SetPlayerNickName (string nickname)
        {
            PhotonNetwork.LocalPlayer.NickName = nickname;
        }
    }
}
