#if UNITY_5_3_OR_NEWER
using System.IO;
using UnityEngine;
#endif

namespace Kalkatos
{
	public class Storage
	{
#if UNITY_5_3_OR_NEWER
		private static UnityStorage storage = new UnityStorage();
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

#if UNITY_5_3_OR_NEWER
	public class UnityStorage
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
						saveMethod = new JsonMethod();
						break;
				}
			}
		}

		private static UntiySaveMethod saveMethod = new PlayerPrefsMethod();

		public void Save (string key, string value) => saveMethod.Save(key, value);
		public void Save (string key, int value) => saveMethod.Save(key, value);
		public void Save (string key, float value) => saveMethod.Save(key, value);
		public string Load (string key, string defaultValue) => saveMethod.Load(key, defaultValue);
		public int Load (string key, int defaultValue) => saveMethod.Load(key, defaultValue);
		public float Load (string key, float defaultValue) => saveMethod.Load(key, defaultValue);
		public void Delete (string key) => saveMethod.Delete(key);

		protected abstract class UntiySaveMethod
		{
			public abstract void Save (string key, string value);
			public abstract void Save (string key, int value);
			public abstract void Save (string key, float value);
			public abstract string Load (string key, string defaultValue);
			public abstract int Load (string key, int defaultValue);
			public abstract float Load (string key, float defaultValue);
			public abstract void Delete (string key);
		}

		private class PlayerPrefsMethod : UntiySaveMethod
		{
			public override void Save (string key, string value) => PlayerPrefs.SetString(key, value);
			public override void Save (string key, int value) => PlayerPrefs.SetInt(key, value);
			public override void Save (string key, float value) => PlayerPrefs.SetFloat(key, value);
			public override string Load (string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);
			public override int Load (string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
			public override float Load (string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
			public override void Delete (string key) => PlayerPrefs.DeleteKey(key);
		}

		private class JsonMethod : UntiySaveMethod
		{
			public override void Save (string key, string value) => throw new System.NotImplementedException();
			public override void Save (string key, int value) => throw new System.NotImplementedException();
			public override void Save (string key, float value) => throw new System.NotImplementedException();
			public override string Load (string key, string defaultValue) => throw new System.NotImplementedException();
			public override int Load (string key, int defaultValue) => throw new System.NotImplementedException();
			public override float Load (string key, float defaultValue) => throw new System.NotImplementedException();
			public override void Delete (string key) => throw new System.NotImplementedException();
		}
	}

	public enum UnityStorageOption
	{
		PlayerPrefs,
		JsonFile
	}
#endif
}
