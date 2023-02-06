using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kalkatos.UnityGame.Screens
{
	public class ScreenManager : MonoBehaviour
	{
		public static ScreenManager Instance { get; private set; }

		[SerializeField] private ScreenTransition[] screenTransitions;

		private static string loadedScene;

		private void Awake ()
		{
			if (Instance == null)
				Instance = this;
			else if (Instance != this)
			{
				Destroy(this);
				return;
			}

			DontDestroyOnLoad(this);
			SceneManager.activeSceneChanged += HandleSceneChanged;
			loadedScene = SceneManager.GetActiveScene().name;
		}

		private void OnDestroy ()
		{
			SceneManager.activeSceneChanged -= HandleSceneChanged;
		}

		private void HandleSceneChanged (Scene oldScene, Scene newScene)
		{
			loadedScene = newScene.name;
		}

		private static void LoadScene (string sceneName)
		{
			if (loadedScene == sceneName)
			{
				Logger.LogWarning($"Trying to load scene that is already loaded: {sceneName} Use ReloadScene() instead.");
				return; 
			}
			SceneManager.LoadScene(sceneName);
			Logger.Log("Loading scene " + sceneName);
		}

		public static void ReloadScene ()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		public static void GoToNextScene (string next = "")
		{
			ScreenTransition transition = Instance.screenTransitions.First((t) => t.Origin.SceneName == loadedScene);
			if (transition == null)
			{
				Logger.LogError($"There is no transition defined for scene {loadedScene}");
				return;
			}
			if (transition.PossibleNext == null || transition.PossibleNext.Length == 0)
			{
				if (!string.IsNullOrEmpty(next))
				{
					Logger.LogWarning($"There is no transition defined for scene {loadedScene} going to scene {next}");
					LoadScene(next);
				}
				return;
			}
			ScenePointer nextScreen = transition.PossibleNext[0];
			if (!string.IsNullOrEmpty(next))
			{
				nextScreen = transition.PossibleNext.First((p) => p.SceneName == next);
				if (nextScreen == null)
				{
					Logger.LogWarning($"Next scene/screen expected is not defined: {next}");
					return;
				}
			}
			LoadScene(nextScreen.SceneName);
		}
	}

	[Serializable]
	public class ScenePointer
	{
		public string SceneName;
	}

	[Serializable]
	public class ScreenTransition
	{
		public ScenePointer Origin;
		public ScenePointer[] PossibleNext;
	}
}
