using System.IO;
using UnityEngine;

namespace Erelia.Core.Utils
{
	/// <summary>
	/// Utility helpers related to file and resource path resolution.
	/// </summary>
	public static class PathUtils
	{
		/// <summary>
		/// Attempts to read a text file from multiple supported locations.
		/// </summary>
		/// <param name="path">
		/// Path to the file. Can be:
		/// <list type="bullet">
		/// <item><description>An absolute filesystem path.</description></item>
		/// <item><description>A Resources-relative path (without extension).</description></item>
		/// <item><description>A path relative to <see cref="Application.streamingAssetsPath"/>.</description></item>
		/// </list>
		/// </param>
		/// <returns>
		/// The file contents as a string if found; otherwise <c>null</c>.
		/// </returns>
		/// <remarks>
		/// Resolution order:
		/// <list type="number">
		/// <item><description>If <paramref name="path"/> is an absolute path and the file exists, read it directly from disk.</description></item>
		/// <item><description>Attempt to load a <see cref="TextAsset"/> from the Unity <c>Resources</c> folder.</description></item>
		/// <item><description>Attempt to read the file from <see cref="Application.streamingAssetsPath"/>.</description></item>
		/// </list>
		/// If none succeed, <c>null</c> is returned.
		/// </remarks>
		public static string ReadTextFromPath(string path)
		{
			// Guard clause for invalid input.
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			// 1) Absolute filesystem path.
			if (Path.IsPathRooted(path) && File.Exists(path))
			{
				return File.ReadAllText(path);
			}

			// 2) Unity Resources folder (path without extension).
			TextAsset asset = Resources.Load<TextAsset>(path);
			if (asset != null)
			{
				return asset.text;
			}

			// 3) StreamingAssets folder (relative path).
			string streamingPath = Path.Combine(Application.streamingAssetsPath, path);
			if (File.Exists(streamingPath))
			{
				return File.ReadAllText(streamingPath);
			}

			// Not found in any supported location.
			return null;
		}
	}
}