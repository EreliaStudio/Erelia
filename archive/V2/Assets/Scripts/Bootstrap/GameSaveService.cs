using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class GameSaveService
{
	private const string SaveDirectoryName = "Saves";
	private const string SaveExtension = ".json";
	private const string SavePrefix = "save_";
	private const string SaveTimestampFormat = "yyyyMMdd_HHmmss_fff";
	private static readonly string[] DebugWords =
	{
		"ember",
		"harbor",
		"quill",
		"signal",
		"thorn",
		"mirror",
		"lantern",
		"gale",
		"atlas",
		"river"
	};

	public static GameData CreateNewGame(out string saveId)
	{
		saveId = GenerateSaveId();
		GameData gameData = new GameData
		{
			DebugMessage = GenerateDebugMessage()
		};

		try
		{
			Save(saveId, gameData);
			Debug.Log($"[GameSaveService] Created save '{saveId}' at '{GetSavePath(saveId)}'.");
			return gameData;
		}
		catch (Exception exception)
		{
			Debug.LogError($"[GameSaveService] Failed to create a new game save. {exception.Message}");
			saveId = string.Empty;
			return null;
		}
	}

	public static string[] GetAvailableSaveIds()
	{
		string saveDirectory = GetSaveDirectory();
		if (!Directory.Exists(saveDirectory))
		{
			return Array.Empty<string>();
		}

		string[] savePaths = Directory.GetFiles(saveDirectory, $"*{SaveExtension}", SearchOption.TopDirectoryOnly);
		string[] saveIds = new string[savePaths.Length];

		for (int index = 0; index < savePaths.Length; index++)
		{
			saveIds[index] = Path.GetFileNameWithoutExtension(savePaths[index]);
		}

		Array.Sort(saveIds, StringComparer.Ordinal);
		Array.Reverse(saveIds);
		return saveIds;
	}

	public static string GetDisplayName(string saveId)
	{
		if (string.IsNullOrWhiteSpace(saveId))
		{
			return "Unnamed Save";
		}

		if (TryParseSaveTimestamp(saveId, out DateTime saveTimestamp))
		{
			return saveTimestamp.ToLocalTime().ToString("'Save' yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		return saveId.Replace('_', ' ');
	}

	public static bool TryLoad(string saveId, out GameData gameData)
	{
		gameData = null;

		if (string.IsNullOrWhiteSpace(saveId))
		{
			return false;
		}

		string savePath = GetSavePath(saveId);
		if (!File.Exists(savePath))
		{
			Debug.LogWarning($"[GameSaveService] Save '{saveId}' does not exist at '{savePath}'.");
			return false;
		}

		try
		{
			string json = File.ReadAllText(savePath);
			GameData loadedData = JsonUtility.FromJson<GameData>(json);
			if (loadedData == null)
			{
				Debug.LogWarning($"[GameSaveService] Save '{saveId}' could not be deserialized.");
				return false;
			}

			gameData = loadedData;
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogError($"[GameSaveService] Failed to load save '{saveId}'. {exception.Message}");
			return false;
		}
	}

	private static string GenerateDebugMessage()
	{
		List<string> words = new List<string>(DebugWords);
		System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

		for (int index = 0; index < words.Count; index++)
		{
			int swapIndex = random.Next(index, words.Count);
			string temporaryWord = words[index];
			words[index] = words[swapIndex];
			words[swapIndex] = temporaryWord;
		}

		return string.Join(" ", words);
	}

	private static void Save(string saveId, GameData gameData)
	{
		Directory.CreateDirectory(GetSaveDirectory());
		string savePath = GetSavePath(saveId);
		string json = JsonUtility.ToJson(gameData, true);
		File.WriteAllText(savePath, json);
	}

	private static string GenerateSaveId()
	{
		string baseSaveId = $"{SavePrefix}{DateTime.UtcNow.ToString(SaveTimestampFormat, CultureInfo.InvariantCulture)}";
		string saveId = baseSaveId;
		int suffix = 1;

		while (File.Exists(GetSavePath(saveId)))
		{
			saveId = $"{baseSaveId}_{suffix}";
			suffix++;
		}

		return saveId;
	}

	private static string GetSaveDirectory()
	{
		return Path.Combine(Application.persistentDataPath, SaveDirectoryName);
	}

	private static string GetSavePath(string saveId)
	{
		return Path.Combine(GetSaveDirectory(), saveId + SaveExtension);
	}

	private static bool TryParseSaveTimestamp(string saveId, out DateTime saveTimestamp)
	{
		saveTimestamp = default;
		if (!saveId.StartsWith(SavePrefix, StringComparison.Ordinal))
		{
			return false;
		}

		string timestamp = saveId.Substring(SavePrefix.Length);
		if (timestamp.Length < SaveTimestampFormat.Length)
		{
			return false;
		}

		timestamp = timestamp.Substring(0, SaveTimestampFormat.Length);
		return DateTime.TryParseExact(
			timestamp,
			SaveTimestampFormat,
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
			out saveTimestamp);
	}
}
