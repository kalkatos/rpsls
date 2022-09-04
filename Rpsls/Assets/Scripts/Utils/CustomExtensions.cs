using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos
{
    public static class CustomExtensions
    {
        #region MonoBehaviour ================================================

        private static IEnumerator Wait (float time, Action callback)
        {
            float startTime = Time.time;
            while (Time.time - startTime < time)
                yield return null;
            callback?.Invoke();
        }

        public static void Wait (this MonoBehaviour mono, float time, Action callback = null)
        {
            mono.StartCoroutine(Wait(time, callback));
        }

        public static void Log (this MonoBehaviour mono, string message)
        {
            Debug.Log($"[{mono.GetType().Name}] {message}");
        }

        public static void LogError (this MonoBehaviour mono, string message)
        {
            Debug.LogError($"[{mono.GetType().Name}] {message}");
        }

        public static void LogWarning (this MonoBehaviour mono, string message)
        {
            Debug.LogWarning($"[{mono.GetType().Name}] {message}");
        }

        #endregion

        #region Dictionary & List ================================================

        public static Dictionary<string, T> UpdateOrAdd<T> (this Dictionary<string, T> dict, string key, T value)
        {
            if (ReferenceEquals(dict, null))
                dict = new Dictionary<string, T>();
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return dict;
            }
            dict.Add(key, value);
            return dict;
        }

        public static Dictionary<string, T> UpdateOrAdd<T> (this Dictionary<string, T> dict, Dictionary<string, T> otherDict)
        {
            foreach (var item in otherDict)
                dict = dict.UpdateOrAdd(item.Key, item.Value);
            return dict;
        }

        public static Dictionary<string, T> CloneDictionary<T> (this Dictionary<string, T> source)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            foreach (var item in source)
                dict.Add(item.Key, item.Value);
            return dict;
        }

        public static List<T> CloneList<T> (this List<T> source)
        {
            List<T> list = new List<T>();
            foreach (var item in source)
                list.Add(item);
            return list;
        }

        public static object[] ToObjArray<T> (this Dictionary<string, T> dict)
        {
            object[] result = new object[dict.Count * 2];
            int index = 0;
            foreach (var item in dict)
            {
                result[index] = item.Key;
                result[index + 1] = item.Value;
                index += 2;
            }
            return result;
        }

        #endregion

        #region Arrays ================================================

        public static Dictionary<string, object> ToDictionary (this object[] objArray)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < objArray.Length; i++)
            {
                if (i % 2 == 0)
                    dict.Add(objArray[i].ToString(), null);
                else
                    dict[objArray[i - 1].ToString()] = objArray[i];
            }
            return dict;
        }

        public static Dictionary<string, T> ToDictionary<T> (this object[] objArray)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            string currentKey = "";
            for (int i = 0; i < objArray.Length; i++)
            {
                if (i % 2 == 0)
                    currentKey = objArray[i].ToString();
                else
                    dict.Add(currentKey, (T)objArray[i]);
            }
            return dict;
        }

        public static bool ContainsKey (this object[] objArray, string key)
        {
            for (int i = 0; i < objArray.Length; i++)
            {
                object item = objArray[i];
                if (item != null && item is string && (string)item == key)
                    return true;
            }
            return false;
        }

        public static object GetByKey (this object[] objArray, string key)
        {
            for (int i = 0; i < objArray.Length; i++)
            {
                object item = objArray[i];
                if (item != null && item is string && (string)item == key && objArray.Length > i + 1)
                    return objArray[i + 1];
            }
            return null;
        }

        //public static void SetByKey (this object[] objArray, string key, object value)
        //{
        //    if (ReferenceEquals(objArray, null))
        //    {
        //        objArray = new object[2] { key, value };
        //        return;
        //    }
        //    for (int i = 0; i < objArray.Length; i++)
        //    {
        //        object item = objArray[i];
        //        if (item != null && item is string && (string)item == key && objArray.Length > i + 1)
        //            objArray[i + 1] = value;
        //    }
        //}

        public static object[] CloneWithChange (this object[] objArray, string key, object value)
        {
            object[] newArray;
            if (ReferenceEquals(objArray, null))
            {
                newArray = new object[2];
                newArray[0] = key;
                newArray[1] = value;
                return newArray;
            }
            if (objArray.ContainsKey(key))
                for (int i = 0; i < objArray.Length; i++)
                {
                    object item = objArray[i];
                    if (item != null && item is string && (string)item == key && objArray.Length > i + 1)
                    {
                        objArray[i + 1] = value;
                        return objArray; 
                    }
                }
            newArray = new object[objArray.Length + 2];
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i].ToString() == key && i + 1 < objArray.Length)
                {
                    objArray[i + 1] = value;
                    return objArray;
                }
                newArray[i] = objArray[i];
            }
            newArray[newArray.Length - 2] = key;
            newArray[newArray.Length - 1] = value;
            return newArray;
        }

        public static string AsKeyValueString (this object[] objArray)
        {
            string result = "";
            if (!ReferenceEquals(objArray, null))
            {
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (i > 0)
                            result += ";";
                        result += $"[{objArray[i]}] ";
                    }
                    else
                        result += objArray[i].ToString();
                }
            }
            return result;
        }

        public static T[] SubArray<T> (this T[] array, int offset)
        {
            int length = array.Length - offset;
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        public static bool Contains<T> (this T[] array, T value)
        {
            return Array.IndexOf(array, value) >= 0;
        }

        #endregion
    }
}
