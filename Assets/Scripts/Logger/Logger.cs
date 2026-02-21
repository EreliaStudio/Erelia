using System;
using UnityEngine;

namespace Erelia
{
	public static class Logger
	{
		static bool IsLogging = false;
		public static void Log(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (IsLogging == true)
			{
				Debug.Log(message);
			}
#endif
		}

		public static void RaiseWarning(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (IsLogging == true)
			{
				Debug.LogWarning(message);
			}
#endif
		}

		public static void RaiseError(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (IsLogging == true)
			{
				Debug.LogError(message);
			}
#endif
		}

		public static void RaiseException(string message)
		{
			RaiseError(message);
			throw new Exception(message);
		}
	}
}
