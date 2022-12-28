using UnityEngine;

namespace Kalkatos.UnityGame.Systems
{
	public class Logger
	{
		public static void Log (string msg) 
		{
			Debug.Log(msg);
		}

		public static void LogWarning (string msg)
		{
			Debug.LogWarning(msg);
		}

		public static void LogError (string msg)
		{
			Debug.LogError(msg);
		}
	}
}
