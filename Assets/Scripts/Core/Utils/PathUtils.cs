using System.IO;
using UnityEngine;

namespace Erelia.Core.Utils
{
	public static class PathUtils
	{
		public static string ReadTextFromPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			if (Path.IsPathRooted(path) && File.Exists(path))
			{
				return File.ReadAllText(path);
			}

			TextAsset asset = Resources.Load<TextAsset>(path);
			if (asset != null)
			{
				return asset.text;
			}

			string streamingPath = Path.Combine(Application.streamingAssetsPath, path);
			if (File.Exists(streamingPath))
			{
				return File.ReadAllText(streamingPath);
			}

			return null;
		}
	}
}
