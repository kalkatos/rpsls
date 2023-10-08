using System;
using System.Collections;
using UnityEngine;

namespace Kalkatos.UnityGame
{
    public static class MonoExtensions
    {
        public static IEnumerator WaitCoroutine (float time, Action callback)
        {
            float endTime = Time.time + time;
            while (Time.time < endTime)
                yield return null;
            callback?.Invoke();
		}

        public static void Wait (this MonoBehaviour mono, float time, Action callback)
        {
            mono.StartCoroutine(WaitCoroutine(time, callback));
		}
    }
}