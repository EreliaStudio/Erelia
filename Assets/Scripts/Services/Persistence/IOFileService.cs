using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class IOFileService
{
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

	public bool TrySave(JObject p_json)
	{
		if (p_json == null)
		{
			return false;
		}

		try
		{
			Directory.CreateDirectory(saveDirectoryPath);

			string json = p_json.ToString(Formatting.Indented);
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

	public bool TryLoad(out JObject p_json)
	{
		p_json = null;

		if (!File.Exists(SaveFilePath))
		{
			return false;
		}

		try
		{
			string text = File.ReadAllText(SaveFilePath, Encoding.UTF8);
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			p_json = JObject.Parse(text);
			return p_json != null;
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
