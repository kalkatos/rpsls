using System;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using Kalkatos.UnityGame.Signals;

namespace Kalkatos.UnityGame.Screens
{
	[ExecuteAlways]
	public class ScreenManager : MonoBehaviour
	{
		public static ScreenManager Instance { get; private set; }

		[SerializeField] private ScreenTransition[] screenTransitions;

		[SerializeField, HideInInspector] private ScreenSignal[] screenSignals;

		private static string loadedScene;

		private void Awake ()
		{
			if (Application.isPlaying)
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
			else
			{
#if UNITY_EDITOR
				screenSignals = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(ScreenSignal))
					.Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
					.Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<ScreenSignal>(x)).ToArray();
#endif
			}
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
				return;
			SceneManager.LoadScene(sceneName);
			Debug.Log("Loading scene " + sceneName);
		}

		private static ScreenSignal GetScreenSignal (string name)
		{
			return Instance.screenSignals?.First(x => x.name == name);
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

		public static void SetScreenStatus (string name, bool b)
		{
			var screen = GetScreenSignal(name);
			if (screen != null)
				screen.EmitWithParam(b);
			else
				Logger.LogError($"Couldn't find screen {name}");
		}

		public static void OpenScreen (string name)
		{
			SetScreenStatus(name, true);
		}

		public static void CloseScreen (string name)
		{
			SetScreenStatus(name, false);
		}
	}

	[Serializable]
	public class ScenePointer
	{
		[Scene] public string SceneName;
	}

	[Serializable]
	public class ScreenTransition
	{
		public ScenePointer Origin;
		public ScenePointer[] PossibleNext;
	}
}
