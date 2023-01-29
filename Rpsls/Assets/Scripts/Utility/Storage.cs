using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Kalkatos
{
	public class Storage
	{
#if UNITY_5_3_OR_NEWER
		private static UnityStorage storage = new UnityStorage();
#else
		private static FileStorage storage = new FileStorage();
#endif

		public static void Save (string key, string value)
		{
			storage.Save(key, value);
		}

		public static void Save (string key, int value)
		{
			storage.Save(key, value);
		}

		public static void Save (string key, float value)
		{
			storage.Save(key, value);
		}

		public static string Load (string key, string defaultValue)
		{
			return storage.Load(key, defaultValue);
		}

		public static int Load (string key, int defaultValue)
		{
			return storage.Load(key, defaultValue);
		}

		public static float Load (string key, float defaultValue)
		{
			return storage.Load(key, defaultValue);
		}

		public static void Delete (string key)
		{
			storage.Delete(key);
		}
	}

	public interface IStorage
	{
		void Save (string key, string value);
		void Save (string key, int value);
		void Save (string key, float value);
		string Load (string key, string defaultValue);
		int Load (string key, int defaultValue);
		float Load (string key, float defaultValue);
		void Delete (string key);
	}

	public class FileStorage : IStorage
	{
		public string Load (string key, string defaultValue)
		{
			return OpenLoad(key, defaultValue);
		}

		public int Load (string key, int defaultValue)
		{
			string valueStr = OpenLoad(key, defaultValue.ToString());
			return int.Parse(valueStr);
		}

		public float Load (string key, float defaultValue)
		{
			string valueStr = OpenLoad(key, defaultValue.ToString());
			return float.Parse(valueStr);
		}

		public void Save (string key, string value)
		{
			OpenSave(key, value);
		}

		public void Save (string key, int value)
		{
			OpenSave(key, value.ToString());
		}

		public void Save (string key, float value)
		{
			OpenSave(key, value.ToString());
		}

		public void Delete (string key)
		{
			if (File.Exists("player-prefs.json"))
			{
				var prefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("player-prefs.json"));
				if (prefs.ContainsKey(key))
					prefs.Remove(key);
				SaveDictionary(prefs);
			}
		}
		
		private string OpenLoad (string key, string defaultValue)
		{
			if (File.Exists("player-prefs.json"))
			{
				var prefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("player-prefs.json"));
				if (prefs.ContainsKey(key))
					return prefs[key];
				return defaultValue;
			}
			else
				return defaultValue;
		}

		private void OpenSave (string key, string value)
		{
			Dictionary<string, string> prefs = null;
			if (File.Exists("player-prefs.json"))
			{
				prefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("player-prefs.json"));
				if (prefs.ContainsKey(key))
					prefs[key] = value;
				else
					prefs.Add(key, value);
			}
			else
				prefs = new Dictionary<string, string> { { key, value } };
			SaveDictionary(prefs);
		}

		private void SaveDictionary (Dictionary<string, string> dict)
		{
			File.WriteAllText("player-prefs.json", JsonConvert.SerializeObject(dict));
		}
	}

#if UNITY_5_3_OR_NEWER
	public class UnityStorage : IStorage
	{
		private static UnityStorageOption option;
		public static UnityStorageOption Option
		{
			get => option;
			set
			{
				option = value;
				switch (option)
				{
					case UnityStorageOption.PlayerPrefs:
						saveMethod = new PlayerPrefsMethod();
						break;
					case UnityStorageOption.JsonFile:
						saveMethod = new FileStorage();
						break;
				}
			}
		}

		private static IStorage saveMethod = new FileStorage();

		public void Save (string key, string value) => saveMethod.Save(key, value);
		public void Save (string key, int value) => saveMethod.Save(key, value);
		public void Save (string key, float value) => saveMethod.Save(key, value);
		public string Load (string key, string defaultValue) => saveMethod.Load(key, defaultValue);
		public int Load (string key, int defaultValue) => saveMethod.Load(key, defaultValue);
		public float Load (string key, float defaultValue) => saveMethod.Load(key, defaultValue);
		public void Delete (string key) => saveMethod.Delete(key);

		private class PlayerPrefsMethod : IStorage
		{
			public void Save (string key, string value) => PlayerPrefs.SetString(key, value);
			public void Save (string key, int value) => PlayerPrefs.SetInt(key, value);
			public void Save (string key, float value) => PlayerPrefs.SetFloat(key, value);
			public string Load (string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);
			public int Load (string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
			public float Load (string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
			public void Delete (string key) => PlayerPrefs.DeleteKey(key);
		}

		private class JsonMethod : IStorage
		{
			public void Save (string key, string value) => throw new System.NotImplementedException();
			public void Save (string key, int value) => throw new System.NotImplementedException();
			public void Save (string key, float value) => throw new System.NotImplementedException();
			public string Load (string key, string defaultValue) => throw new System.NotImplementedException();
			public int Load (string key, int defaultValue) => throw new System.NotImplementedException();
			public float Load (string key, float defaultValue) => throw new System.NotImplementedException();
			public void Delete (string key) => throw new System.NotImplementedException();
		}
	}

	public enum UnityStorageOption
	{
		PlayerPrefs,
		JsonFile
	}
#endif
}
