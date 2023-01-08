using System;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;
using Kalkatos.UnityGame.Signals;
using System.Collections.Generic;
using UnityEngine.Events;
using Newtonsoft.Json;

namespace Kalkatos.UnityGame.Screens
{
	[ExecuteAlways]
	public class ScreenManager : MonoBehaviour
	{
		public static ScreenManager Instance { get; private set; }

		[SerializeField] private ScreenTransition[] screenTransitions;

		[SerializeField] private ScreenSignal[] screenSignals;

		private static string loadedScene;
		private static Dictionary<ScreenSignal, UnityAction<bool>> signalDict = new Dictionary<ScreenSignal, UnityAction<bool>>();

		private void Awake ()
		{
			if (!Application.isPlaying)
			{
				LoadScreenSignals();
				return;
			}

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

			Logger.Log("screenSignals = " + JsonConvert.SerializeObject(screenSignals, Formatting.Indented));
			if (screenSignals != null)
				for (int i = 0; i < screenSignals.Length; i++)
				{
					string name = screenSignals[i].name;
					UnityAction<bool> ev = (b) =>
					{
						Logger.Log("[ScreenManager] Lambda - received signal to change to screen: " + name);
						ScreenSignalReceived(name, b);
					};
					screenSignals[i].OnSignalEmittedWithParam.AddListener(ev);
					signalDict.Add(screenSignals[i], ev);
				}
		}

		private void OnDestroy ()
		{
			SceneManager.activeSceneChanged -= HandleSceneChanged;
			foreach (var item in signalDict)
				item.Key.OnSignalEmittedWithParam.RemoveListener(item.Value);
		}

		private void LoadScreenSignals ()
		{
#if UNITY_EDITOR
			screenSignals = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(ScreenSignal))
				.Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
				.Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<ScreenSignal>(x)).ToArray();
			UnityEditor.EditorUtility.SetDirty(gameObject);
			Logger.Log("screenSignals = " + JsonConvert.SerializeObject(screenSignals, Formatting.Indented));
#endif
		}

		private void ScreenSignalReceived (string signal, bool b)
		{
			Scene scene = SceneManager.GetSceneByName(signal);
			Logger.Log($"Screen signal received: {signal} {b} || Scene: {scene}");
			if (scene.IsValid())
				LoadScene(signal);
		}

		private void HandleSceneChanged (Scene oldScene, Scene newScene)
		{
			loadedScene = newScene.name;
		}

		private static void LoadScene (string sceneName)
		{
			if (loadedScene == sceneName)
			{
				Debug.Log($"Trying to load scene that is already loaded: {sceneName} Use ReloadScene() instead.");
				return; 
			}
			SceneManager.LoadScene(sceneName);
			Debug.Log("Loading scene " + sceneName);
		}

		private static void ReloadScene ()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

		public static void OpenScreen (ScreenSignal signal)
		{
			signal.EmitWithParam(true);
		}

		public static void CloseScreen (string name)
		{
			SetScreenStatus(name, false);
		}

		public static void CloseScreen (ScreenSignal signal)
		{
			signal.EmitWithParam(false);
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
