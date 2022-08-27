using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

namespace Kalkatos.Rpsls
{
    public class LobbyScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField nicknameField;
        [SerializeField] private TMP_InputField roomNameField;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button createRoomButton;
        [Header("Events")]
        [SerializeField] private UnityEvent OnFailedToJoinRoom;

        private void Awake ()
        {
            NetworkManager.OnFindMatchFailure += HandleFindMatchFailure;

            roomNameField.onValueChanged.AddListener(OnRoomNameBeignTyped);
            nicknameField.onEndEdit.AddListener(OnNicknameFieldChanged);
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        }

        private void OnDestroy ()
        {
            NetworkManager.OnFindMatchFailure -= HandleFindMatchFailure;

            roomNameField.onValueChanged.RemoveListener(OnRoomNameBeignTyped);
            nicknameField.onEndEdit.RemoveListener(OnNicknameFieldChanged);
            joinRoomButton.onClick.RemoveListener(OnJoinRoomButtonClicked);
            createRoomButton.onClick.RemoveListener(OnCreateRoomButtonClicked);
        }

        private void Start ()
        {
            string nickname = SaveManager.GetNickname();
            if (string.IsNullOrEmpty(nickname))
                nickname = "Guest-" + GetRandomNumber();
            nicknameField.text = nickname;
            SetNickname(nickname);
            if (!NetworkManager.Instance.IsConnected)
            {
                joinRoomButton.interactable = false;
                createRoomButton.interactable = false;
            }
        }

        private void HandleFindMatchFailure (object parameter)
        {
            OnFailedToJoinRoom?.Invoke();
        }

        private void OnRoomNameBeignTyped (string newName)
        {
            roomNameField.text = newName.ToUpper();
        }

        private void OnNicknameFieldChanged (string newNick)
        {
            SetNickname(newNick);
        }

        private void OnJoinRoomButtonClicked ()
        {
            NetworkManager.Instance.FindMatch(roomNameField.text);
        }

        private void OnCreateRoomButtonClicked ()
        {
            NetworkManager.Instance.FindMatch();
        }

        private void SetNickname (string nickname)
        {
            SaveManager.SaveNickname(nickname);
            NetworkManager.Instance.SetPlayerName(nickname);
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
