// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Kalkatos
{
	/// <summary>
	/// Provides logging solutions for any platform.
	/// </summary>
	public class Logger
	{
#if UNITY_5_3_OR_NEWER
		private static ILoggger log = new UnityLogger();
#else
		private static ILoggger log = new BaseLogger();
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

	public interface ILoggger
	{
		void Log (string msg);
		void LogWarning (string msg);
		void LogError (string msg);
	}

	public class BaseLogger : ILoggger
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
	public class UnityLogger : ILoggger
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
