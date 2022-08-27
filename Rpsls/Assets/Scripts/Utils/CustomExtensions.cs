using System;
using System.Collections;
using UnityEngine;

namespace Kalkatos.Rpsls
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
    }
}
