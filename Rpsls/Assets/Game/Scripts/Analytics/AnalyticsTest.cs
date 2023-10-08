// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using UnityEngine;

namespace Kalkatos.Analytics.Unity
{
    public class AnalyticsTest : IAnalyticsSender
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad ()
        {
            Analytics.Sender = new AnalyticsTest();
        }

        public void SendEvent (string name)
        {
            Logger.Log("[AnalyticsTest] Sent: " + name);
        }

        public void SendEventWithNumber (string name, float value)
        {
            Logger.Log("[AnalyticsTest] Sent: " + name);
        }

        public void SendEventWithString (string name, string str)
        {
            Logger.Log("[AnalyticsTest] Sent: " + name);
        }
    }
}