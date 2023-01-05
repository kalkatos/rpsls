using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

namespace Kalkatos.UnityGame.Systems
{
	public class ScreenManager : MonoBehaviour
	{
		public static ScreenManager Instance { get; private set; }

		public static Action<string> OnStarted;
		public static Action<string> OnEnded;

		[SerializeField] private ScreenTransition[] screenTransitions;

		private static string loadedScene;

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
			OnStarted?.Invoke(loadedScene);
		}

		private static void LoadScene (string sceneName)
		{
			if (loadedScene == sceneName)
				return;
			SceneManager.LoadScene(sceneName);
			Debug.Log("Loading scene " + sceneName);
		}

		private static void LoadSceneOrScreen (ScreenPointer pointer)
		{
			if (pointer.IsScene)
				LoadScene(pointer.Value);
			else
				OnStarted?.Invoke(pointer.Value);
		}

		public static void EndScreen (string current, string next = "")
		{
			ScreenTransition transition = Instance.screenTransitions.First((t) => t.Origin.Value == current);
			if (transition == null)
			{
				Logger.LogError($"Invoking transition from an unknown scene/screen: {current}");
				return;
			}
			if (transition.PossibleNext == null || transition.PossibleNext.Length == 0)
			{
				if (!string.IsNullOrEmpty(next))
					Logger.LogWarning($"Next scene/screen expected is not defined: {next}");
				return;
			}
			ScreenPointer nextScreen = transition.PossibleNext[0];
			if (!string.IsNullOrEmpty(next))
			{
				nextScreen = transition.PossibleNext.First((p) => p.Value == next);
				if (nextScreen == null)
				{
					Logger.LogWarning($"Next scene/screen expected is not defined: {next}");
					return;
				}
			}
			OnEnded?.Invoke(current);
			LoadSceneOrScreen(nextScreen);
		}
	}

	[Serializable]
	public class ScreenPointer
	{
		public bool IsScene;
		[Scene] public string SceneName;
		public string ScreenName;
		public string Value { get { if (IsScene) return SceneName; return ScreenName; } }
	}

	[Serializable]
	public class ScreenTransition
	{
		public ScreenPointer Origin;
		public ScreenPointer[] PossibleNext;
	}
}
