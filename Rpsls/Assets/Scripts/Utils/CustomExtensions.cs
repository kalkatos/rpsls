using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kalkatos
{
    public static class CustomExtensions
    {
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
    }
}
