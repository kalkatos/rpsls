using UnityEngine;

namespace Kalkatos.UnityGame.Systems
{
	public class Storage
	{
		public static void Save (string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public static void Save (string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public static void Save (string key, float value)
		{
			PlayerPrefs.SetFloat(key, value);
		}

		public static string Load (string key, string defaultValue)
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public static int Load (string key, int defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public static void Load (string key, float defaultValue)
		{
			PlayerPrefs.GetFloat(key, defaultValue);
		}
	}

	public class Logger
	{
		public static void Log (string msg) 
		{
			Debug.Log(msg);
		}

		public static void LogWarning (string msg)
		{
			Debug.LogWarning(msg);
		}

		public static void LogError (string msg)
		{
			Debug.LogError(msg);
		}
	}
}
