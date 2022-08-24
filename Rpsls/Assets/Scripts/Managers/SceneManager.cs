using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

namespace Kalkatos.Rpsls
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        [SerializeField, Scene] private string connectingScene;
        [SerializeField, Scene] private string lobbyScene;
        [SerializeField, Scene] private string roomScene;
        [SerializeField, Scene] private string mainScene;

        private static string currentScene;

        private void Awake ()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(Instance);
                return;
            }

            currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            DontDestroyOnLoad(this);
        }

        private static void LoadScene (string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        public static void EndScene (object parameter = null)
        {
            if (currentScene == Instance.connectingScene)
            {
                LoadScene(Instance.lobbyScene); 
            }
            else if (currentScene == Instance.lobbyScene)
            {
                LoadScene(Instance.roomScene); 
            }
            else if (currentScene == Instance.roomScene)
            {
                LoadScene(Instance.mainScene); 
            }
        }
    }
}
