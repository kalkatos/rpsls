using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Kalkatos.Rpsls
{
    public class GameScreen : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly] private Button exitButton;

        private void Awake ()
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnDestroy ()
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void OnExitButtonClicked ()
        {
            GameManagerClient.ExitRoom();
        }
    }
}
