using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameLoadLogger
{
	private const string MainMenuSceneName = "MainMenu";
	private static bool isInitialized;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		if (isInitialized)
		{
			return;
		}

		SceneManager.sceneLoaded += HandleSceneLoaded;
		isInitialized = true;
	}

	private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (string.Equals(scene.name, MainMenuSceneName, StringComparison.Ordinal))
		{
			return;
		}

		if (!GameSession.TryConsumePendingLoad(out string saveId, out GameData gameData))
		{
			return;
		}

		Debug.Log($"[GameLoad] Loaded save '{saveId}' with debug message: {gameData.DebugMessage}");
	}
}
