// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

#if UNITY_2018_1_OR_NEWER && KALKATOS_SCRIPTABLE

using UnityEngine;
using Kalkatos.UnityGame.Scriptable;

namespace Kalkatos.Analytics.Unity
{
    [CreateAssetMenu(fileName = "AnalyticsScriptable", menuName = "Analytics Scriptable")]
    public class AnalyticsScriptable : ScriptableObject
    {
        private string key;

        public void SetKey (string key)
        {
            this.key = key;
        }

        public void SendEvent (string name)
        {
            Analytics.SendEvent(name);
        }

        public void SendEvent (SignalString signalString)
        {
            Analytics.SendEvent(signalString.Value);
        }

        public void SendUniqueEvent (string name)
        {
            Analytics.SendUniqueEvent(name);
        }

        public void SendUniqueEvent (SignalString signalString)
        {
            Analytics.SendUniqueEvent(signalString.Value);
        }

        public void SendEventWithString (string key, string value)
        {
            Analytics.SendEventWithString(key, value);
        }

        public void SendEventWithString (string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning($"[AnalyticsScriptable] Key is not set. Trying to send value {value}");
                return;
            }
            Analytics.SendEventWithString(key, value);
        }

        public void SendEventWithString (SignalString value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning($"[AnalyticsScriptable] Key is not set. Trying to send value {value}");
                return;
            }
            Analytics.SendEventWithString(key, value.Value);
        }

        public void SendEventWithString (SignalState signalState)
        {
            Analytics.SendEventWithString(signalState.Key, signalState.Value);
        }

        public void SendEventWithFloat (string key, float value)
        {
            Analytics.SendEventWithNumber(key, value);
        }

        public void SendEventWithFloat (float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning($"[AnalyticsScriptable] Key is not set. Trying to send value {value}");
                return;
            }
            Analytics.SendEventWithNumber(key, value);
        }

        public void SendEventWithFloat (SignalFloat value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning($"[AnalyticsScriptable] Key is not set. Trying to send value {value}");
                return;
            }
            Analytics.SendEventWithNumber(key, value.Value);
        }

        public void SendEventWithFloat (SignalState signalState)
        {
            if (!float.TryParse(signalState.Value, out float value))
            {
                Logger.LogWarning($"[AnalyticsScriptable] Value is not float. Trying to send value {signalState.Key}={signalState.Value}");
                return;
            }
            Analytics.SendEventWithNumber(key, value);
        }
    }
}

#endif