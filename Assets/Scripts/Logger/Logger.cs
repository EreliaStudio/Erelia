using System;
using UnityEngine;

namespace Erelia
{
	public static class Logger
	{
		public static void Log(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.Log(message);
#endif
		}

		public static void RaiseWarning(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.LogWarning(message);
#endif
		}

		public static void RaiseError(string message)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.LogError(message);
#endif
		}

		public static void RaiseException(string message)
		{
			Debug.LogError(message);
			throw new Exception(message);
		}
	}
}
