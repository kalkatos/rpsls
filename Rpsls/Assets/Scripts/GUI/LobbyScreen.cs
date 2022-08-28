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
        [SerializeField] private UnityEvent OnFailedToJoin;

        private void Awake ()
        {
            LobbyManager.OnFindMatchFailure += HandleFindMatchFailure;
            roomNameField.onValueChanged.AddListener(OnRoomNameBeignTyped);
            nicknameField.onEndEdit.AddListener(OnNicknameFieldChanged);
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        }

        private void OnDestroy ()
        {
            LobbyManager.OnFindMatchFailure -= HandleFindMatchFailure;
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
            if (!LobbyManager.Instance.IsConnected)
            {
                joinRoomButton.interactable = false;
                createRoomButton.interactable = false;
            }
        }

        private void HandleFindMatchFailure ()
        {
            OnFailedToJoin?.Invoke();
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
            LobbyManager.Instance.FindMatch(roomNameField.text);
        }

        private void OnCreateRoomButtonClicked ()
        {
            LobbyManager.Instance.FindMatch();
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
