using System.IO;
using UnityEngine;

namespace Erelia.Core.Utils
{
	public static class JsonIO
	{
		public static void Save<T>(string path, T data, bool prettyPrint = true)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new System.ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			string json = JsonUtility.ToJson(data, prettyPrint);
			File.WriteAllText(path, json);
		}

		public static T Load<T>(string path)
		{
			string json = PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				return default;
			}

			return JsonUtility.FromJson<T>(json);
		}

		public static bool TryLoad<T>(string path, out T data) where T : class
		{
			data = null;
			string json = PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				return false;
			}

			data = JsonUtility.FromJson<T>(json);
			return data != null;
		}
	}
}
