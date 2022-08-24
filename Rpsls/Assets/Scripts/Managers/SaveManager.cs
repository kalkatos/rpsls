using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos.Rpsls
{
    public static class SaveManager
    {
#region Save Implementation
        public static bool HasKey (string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void SaveInt (string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static void SaveString (string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static int GetInt (string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static string GetString (string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
#endregion

        public static string GetNickname ()
        {
            return GetString("Nickname", string.Empty);
        }

        public static void SaveNickname (string nickname)
        {
            SaveString("Nickname", nickname);
        }
    }
}
