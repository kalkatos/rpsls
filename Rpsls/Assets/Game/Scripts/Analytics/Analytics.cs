// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Kalkatos.Analytics
{
    public class Analytics
    {
		public static bool SendDebug;
		public static IAnalyticsSender Sender;

		public static void SendEvent (string name)
        {
			if (!CheckSender())
				return;
			if (SendDebug)
				Logger.Log($"[Analytics] Event: {name}");
            Sender.SendEvent(name);
        }

		public static void SendEventWithString (string name, string str)
        {
			if (!CheckSender())
				return;
			if (SendDebug)
				Logger.Log($"[Analytics] Event: {name} with value: {str}");
			Sender.SendEventWithString(name, str); 
        }

		public static void SendEventWithNumber (string name, float value)
        {
			if (!CheckSender())
				return;
			if (SendDebug)
				Logger.Log($"[Analytics] Event: {name} with value: {value}");
			Sender.SendEventWithNumber(name, value); 
        }

		public static void SendUniqueEvent (string name, string optValue = null)
        {
			if (!CheckSender())
				return;
			string key = string.IsNullOrEmpty(optValue) ? name : $"{name}&{optValue}";
			if (!string.IsNullOrEmpty(Storage.Load(key, null)))
				return;
			SendEventWithString(name, optValue);
			Storage.Save(key, "");
        }

		public static void SendEventWithFilter (string name, float value, params float[] orderedFilterTiers)
        {
			if (!CheckSender())
				return;
			if (orderedFilterTiers == null)
            {
				Logger.LogError("[Analytics] orderedFilterTiers is null.");
                return; 
            }
            for (int i = 0; i < orderedFilterTiers.Length; i++)
            {
				if (value <= orderedFilterTiers[i])
				{
					SendEventWithNumber(name, orderedFilterTiers[i]);
					return;
				}
            }
        }

		private static bool CheckSender ()
        {
			if (Sender == null)
            {
                Logger.LogError("Analytics Sender is null. Please set it first");
				return false;
            }
			return true;
        }
	}
}