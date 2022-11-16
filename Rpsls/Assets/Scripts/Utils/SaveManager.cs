using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Kalkatos
{
    public static class SaveManager
    {
        private static ISaveImplementation save = new SaveInScriptable();

        private static string CloneArgument =>
#if UNITY_EDITOR
            ClonesManager.IsClone() ? ClonesManager.GetArgument() : "";
#else
            "";
#endif

		private static string nicknameKey => "Nickname" + CloneArgument;

        public static bool HasKey (string key)
        {
            return save.HasKey(key);
        }

        public static void SaveInt (string key, int value)
        {
            save.SaveInt(key, value);
        }

        public static void SaveString (string key, string value)
        {
            save.SaveString(key, value);
        }

        public static int GetInt (string key, int defaultValue)
        {
            return save.GetInt(key, defaultValue);
        }

        public static string GetString (string key, string defaultValue)
        {
            return save.GetString(key, defaultValue);
        }

        public static string GetNickname ()
        {
            return GetString(nicknameKey, string.Empty);
        }

        public static void SaveNickname (string nickname)
        {
            SaveString(nicknameKey, nickname);
        }

        public static void DeleteKey (string key)
        {
            save.DeleteKey(key);
        }
    }

    public interface ISaveImplementation
    {
        public bool HasKey (string key);
        public void SaveInt (string key, int value);
        public int GetInt (string key, int defaultValue);
        public void SaveString (string key, string value);
        public string GetString (string key, string defaultValue);
        public void DeleteKey (string key);
    }

    public class SaveInPlayerPrefs : ISaveImplementation
    {
        public int GetInt (string key, int defaultValue)
        {
            return PlayerPrefs.GetInt (key, defaultValue);
        }

        public string GetString (string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public bool HasKey (string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void SaveInt (string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public void SaveString (string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public void DeleteKey (string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }

    public class SaveInScriptable : ISaveImplementation
    {
        private Dictionary<string, string> dataDict = new Dictionary<string, string>();
        private SaveScriptable scriptable;

        public SaveInScriptable ()
        {
            scriptable = SaveScriptable.Instance;
            if (scriptable != null)
                dataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(scriptable.Data);
            if (dataDict == null)
                dataDict = new Dictionary<string, string>();
        }

        public void DeleteKey (string key)
        {
            dataDict.Remove(key);
            scriptable.Data = JsonConvert.SerializeObject(dataDict, Formatting.Indented);
        }

        public int GetInt (string key, int defaultValue)
        {
            if (HasKey(key))
                return int.Parse(dataDict[key]);
            return defaultValue;
        }

        public string GetString (string key, string defaultValue)
        {
            if (HasKey(key))
                return dataDict[key];
            return defaultValue;
        }

        public bool HasKey (string key)
        {
            return dataDict.ContainsKey(key);
        }

        public void SaveInt (string key, int value)
        {
            if (HasKey(key))
                dataDict[key] = value.ToString();
            else
                dataDict.Add(key, value.ToString());
            scriptable.Data = JsonConvert.SerializeObject(dataDict, Formatting.Indented);
        }

        public void SaveString (string key, string value)
        {
            if (HasKey(key))
                dataDict[key] = value;
            else
                dataDict.Add(key, value);
            scriptable.Data = JsonConvert.SerializeObject(dataDict, Formatting.Indented);
        }
    }

    public class SaveInFile : ISaveImplementation
    {
        private const float timeBetweenSaves = 0.1f;

        private Dictionary<string, object> data = new Dictionary<string, object>();
        private DateTime lastCommitTime;
        private bool waitingForNextCommit;
        private string savePath;

        public SaveInFile ()
        {
            lastCommitTime = DateTime.Now;
            savePath = Application.persistentDataPath + "/SaveFile.json";
            if (File.Exists(savePath))
            {
                string file = File.ReadAllText(savePath);
                if (!string.IsNullOrEmpty(file))
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(file);
            }
        }

        private void Commit ()
        {
            try
            {
                if ((DateTime.Now - lastCommitTime).TotalSeconds > timeBetweenSaves)
                {
                    waitingForNextCommit = false;
                    File.WriteAllText(savePath, JsonConvert.SerializeObject(data, Formatting.Indented));
                    lastCommitTime = DateTime.Now;
                    Debug.Log("Saved file to " + savePath);
                }
                else
                {
                    if (!waitingForNextCommit)
                    {
                        waitingForNextCommit = true;
                        DelayAction(timeBetweenSaves, Commit);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async void DelayAction (float seconds, Action action)
        {
            while ((DateTime.Now - lastCommitTime).TotalSeconds > seconds)
                await Task.Yield();
            action.Invoke();
        }

        private void SaveAny (string key, object value)
        {
            if (HasKey(key))
                data[key] = value;
            else
                data.Add(key, value);
            Commit();
        }

        public int GetInt (string key, int defaultValue)
        {
            if (HasKey(key))
                return (int)data[key];
            return defaultValue;
        }

        public string GetString (string key, string defaultValue)
        {
            if (HasKey(key))
                return (string)data[key];
            return defaultValue;
        }

        public bool HasKey (string key)
        {
            return data.ContainsKey(key);
        }

        public void SaveInt (string key, int value)
        {
            SaveAny(key, value);
        }

        public void SaveString (string key, string value)
        {
            SaveAny(key, value);
        }

        public void DeleteKey (string key)
        {
            data.Remove(key);
            Commit();
        }
    }
}
