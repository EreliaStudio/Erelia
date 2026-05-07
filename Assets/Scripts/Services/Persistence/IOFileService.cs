using System;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class IOFileService
{
	public const int CurrentVersion = GameSaveFileData.CurrentVersion;
	public const string DefaultSaveFileName = "savegame.json";

	private readonly string saveDirectoryPath;
	private readonly string saveFileName;

	public string SaveDirectoryPath => saveDirectoryPath;
	public string SaveFileName => saveFileName;
	public string SaveFilePath => Path.Combine(saveDirectoryPath, saveFileName);

	public IOFileService()
		: this(PersistentDataPath(), DefaultSaveFileName)
	{
	}

	public IOFileService(string p_saveDirectoryPath, string p_saveFileName = DefaultSaveFileName)
	{
		saveDirectoryPath = string.IsNullOrWhiteSpace(p_saveDirectoryPath)
			? PersistentDataPath()
			: p_saveDirectoryPath;
		saveFileName = string.IsNullOrWhiteSpace(p_saveFileName)
			? DefaultSaveFileName
			: p_saveFileName;
	}

	public static string PersistentDataPath()
	{
		return Application.persistentDataPath;
	}

	public bool HasSaveFile()
	{
		return File.Exists(SaveFilePath);
	}

	public bool TrySave(GameSaveFileData p_saveData)
	{
		if (p_saveData == null)
		{
			return false;
		}

		try
		{
			Directory.CreateDirectory(saveDirectoryPath);

			p_saveData.Version = CurrentVersion;
			string json = JsonUtility.ToJson(p_saveData, true);
			string tempFilePath = SaveFilePath + ".tmp";

			File.WriteAllText(tempFilePath, json, Encoding.UTF8);
			if (File.Exists(SaveFilePath))
			{
				File.Delete(SaveFilePath);
			}

			File.Move(tempFilePath, SaveFilePath);
			return true;
		}
		catch (Exception exception)
		{
			Logger.LogError(
				$"[IOFileService] Could not save game file at [{SaveFilePath}]: {exception.Message}",
				Logger.Severity.Warning);
			return false;
		}
	}

	public bool TryLoad(out GameSaveFileData p_saveData)
	{
		p_saveData = null;

		if (!File.Exists(SaveFilePath))
		{
			return false;
		}

		try
		{
			string json = File.ReadAllText(SaveFilePath, Encoding.UTF8);
			if (string.IsNullOrWhiteSpace(json))
			{
				return false;
			}

			p_saveData = JsonUtility.FromJson<GameSaveFileData>(json);
			return p_saveData != null;
		}
		catch (Exception exception)
		{
			Logger.LogError(
				$"[IOFileService] Could not load game file at [{SaveFilePath}]: {exception.Message}",
				Logger.Severity.Warning);
			return false;
		}
	}

	public bool TryDelete()
	{
		try
		{
			if (!File.Exists(SaveFilePath))
			{
				return true;
			}

			File.Delete(SaveFilePath);
			return true;
		}
		catch (Exception exception)
		{
			Logger.LogError(
				$"[IOFileService] Could not delete game file at [{SaveFilePath}]: {exception.Message}",
				Logger.Severity.Warning);
			return false;
		}
	}
}
