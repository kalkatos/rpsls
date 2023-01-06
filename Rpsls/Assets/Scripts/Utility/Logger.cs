using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Kalkatos
{
	public class Logger
	{
#if UNITY_5_3_OR_NEWER
		private static UnityLogger log = new UnityLogger();
#else
		private static BaseLogger log = new BaseLogger();
#endif

		public static void Log (string msg) 
		{
			log.Log(msg);
		}

		public static void LogWarning (string msg)
		{
			log.LogWarning(msg);
		}

		public static void LogError (string msg)
		{
			log.LogError(msg);
		}
	}

	public class BaseLogger
	{
		public void Log (string msg)
		{
			Console.WriteLine(msg);
		}

		public void LogWarning (string msg)
		{
			Console.WriteLine($"[Warning] {msg}");
		}

		public void LogError (string msg)
		{
			Console.WriteLine($"[Error] {msg}");
		}
	}

#if UNITY_5_3_OR_NEWER
	public class UnityLogger
	{
		public void Log (string msg)
		{
			Debug.Log(msg);
		}

		public void LogWarning (string msg)
		{
			Debug.LogWarning(msg);
		}

		public void LogError (string msg)
		{
			Debug.LogError(msg);
		}
	}
#endif
}
