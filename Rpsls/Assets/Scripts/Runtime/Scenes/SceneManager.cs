using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

namespace Kalkatos.Rpsls
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        [SerializeField, Scene] private string connectionScene;
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

            DontDestroyOnLoad(this);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanged;
            currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        private void OnDestroy ()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= HandleSceneChanged;
        }

        private void HandleSceneChanged (Scene oldScene, Scene newScene)
        {
            currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        private static void LoadScene (string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            Debug.Log("Loading scene " + sceneName);
        }

        private static void CheckSceneBeingEnded (string scene)
        {
            if (!string.IsNullOrEmpty(currentScene) && !string.IsNullOrEmpty(scene) && scene != currentScene)
                Debug.LogWarning($"Invoked end of scene {scene} but the current scene is {currentScene}.");
        }

        public static void EndScene (string scene, object parameter = null)
        {
            CheckSceneBeingEnded(scene);
            if (currentScene == Instance.connectionScene)
            {
                LoadScene(Instance.lobbyScene); 
            }
            else if (currentScene == Instance.lobbyScene)
            {
                LoadScene(Instance.roomScene); 
            }
            else if (currentScene == Instance.roomScene)
            {
                if (parameter != null)
                {
                    string destination = (string)parameter;
                    if (destination == "ToGame")
                        LoadScene(Instance.mainScene);
                    else
                        LoadScene(Instance.lobbyScene);
                }
            }
        }
    }
}
