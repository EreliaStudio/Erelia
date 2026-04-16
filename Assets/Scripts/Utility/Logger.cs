using UnityEngine;

public static class Logger
{
	public enum Severity
	{
		Warning,
		Error,
		Critical
	}

	public static void LogDebug(string message, Object context = null)
	{
		if (context != null)
		{
			Debug.Log($"[Logger] {message}", context);
		}
		else
		{
			Debug.Log($"[Logger] {message}");
		}
	}

	public static void LogError(string message, Severity severity = Severity.Error, Object context = null)
	{
		switch (severity)
		{
			case Severity.Warning:
				if (context != null) Debug.LogWarning($"[Logger] {message}", context);
				else Debug.LogWarning($"[Logger] {message}");
				break;
			case Severity.Error:
				if (context != null) Debug.LogError($"[Logger] {message}", context);
				else Debug.LogError($"[Logger] {message}");
				break;
			case Severity.Critical:
				if (context != null) Debug.LogError($"[Logger] CRITICAL: {message}", context);
				else Debug.LogError($"[Logger] CRITICAL: {message}");
				throw new System.Exception($"Critical log: {message}");
		}
	}
}
