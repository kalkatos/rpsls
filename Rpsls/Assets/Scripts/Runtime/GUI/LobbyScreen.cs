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
        [SerializeField] private UnityEvent OnRoomNotFound;
        [SerializeField] private UnityEvent OnRoomClosed;

        private void Awake ()
        {
            LobbyManager.OnRoomNotFoundError += HandleRoomNotFoundError;
            LobbyManager.OnRoomClosedError += HandleRoomClosedError;
            roomNameField.onValueChanged.AddListener(OnRoomNameBeignTyped);
            nicknameField.onEndEdit.AddListener(OnNicknameFieldChanged);
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        }

        private void OnDestroy ()
        {
            LobbyManager.OnRoomNotFoundError -= HandleRoomNotFoundError;
            LobbyManager.OnRoomClosedError -= HandleRoomClosedError;
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
            if (!LobbyManager.IsConnected)
            {
                joinRoomButton.interactable = false;
                createRoomButton.interactable = false;
            }
        }

        private void HandleRoomNotFoundError ()
        {
            OnRoomNotFound?.Invoke();
        }

        private void HandleRoomClosedError ()
        {
            OnRoomClosed?.Invoke();
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
            if (string.IsNullOrEmpty(roomNameField.text))
                LobbyManager.FindMatch("-");
            else
                LobbyManager.FindMatch(roomNameField.text);
        }

        private void OnCreateRoomButtonClicked ()
        {
            LobbyManager.FindMatch();
        }

        private void SetNickname (string nickname)
        {
            SaveManager.SaveNickname(nickname);
            LobbyManager.Instance.SendPlayerName(nickname);
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
