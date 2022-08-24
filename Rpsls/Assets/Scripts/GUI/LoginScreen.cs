using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using Photon.Pun;

namespace Kalkatos.Rpsls
{
    public class LoginScreen : MonoBehaviour
    {
        public UnityEvent OnFailedToJoinRoom;

        [SerializeField] private TMP_InputField nicknameField;
        [SerializeField] private TMP_InputField roomNameField;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button createRoomButton;

        private void Awake ()
        {
            LobbyManager.OnNotConnected += HandleNotConnected;
            LobbyManager.OnFailedToJoinRoom += HandleFailedToJoinRoom;

            roomNameField.onValueChanged.AddListener(HandleRoomNameChanged);
            nicknameField.onEndEdit.AddListener(HandleNicknameEditChanged);
            joinRoomButton.onClick.AddListener(HandleJoinRoomButtonClicked);
            createRoomButton.onClick.AddListener(HandleCreateRoomButtonClicked);
        }

        private void Start ()
        {
            string nickname = SaveManager.GetNickname();
            if (string.IsNullOrEmpty(nickname))
                nickname = "Guest-" + GetRandomNumber();
            nicknameField.text = nickname;
            SetNickname(nickname);
        }

        private void OnDestroy ()
        {
            roomNameField.onValueChanged.RemoveListener(HandleRoomNameChanged);
            nicknameField.onEndEdit.RemoveListener(HandleNicknameEditChanged);
            joinRoomButton.onClick.RemoveListener(HandleJoinRoomButtonClicked);
            createRoomButton.onClick.RemoveListener(HandleCreateRoomButtonClicked);
        }

        private void HandleNotConnected ()
        {
            joinRoomButton.interactable = false;
            createRoomButton.interactable = false;
        }

        private void HandleFailedToJoinRoom ()
        {
            OnFailedToJoinRoom?.Invoke();
        }

        private void HandleRoomNameChanged (string newName)
        {
            roomNameField.text = newName.ToUpper();
        }

        private void HandleNicknameEditChanged (string newNick)
        {
            SetNickname(newNick);
        }

        private void HandleJoinRoomButtonClicked ()
        {
            LobbyManager.TryJoiningRoom(roomNameField.text);
        }

        private void HandleCreateRoomButtonClicked ()
        {
            LobbyManager.CreateRoom();
        }

        private void SetNickname (string nickname)
        {
            SaveManager.SaveNickname(nickname);
            PhotonNetwork.LocalPlayer.NickName = nickname;
            Debug.Log("Nickname set: " + nickname);
        }

        private string GetRandomNumber ()
        {
            string result = "";
            for (int i = 0; i < 7; i++)
                result += Random.Range(0, 10);
            return result;
        }
    }
}
