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
