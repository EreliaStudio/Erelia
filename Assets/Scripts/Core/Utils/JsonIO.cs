using System.IO;
using UnityEngine;

namespace Erelia.Core.Utils
{
	/// <summary>
	/// JSON file I/O helpers built on top of Unity <see cref="JsonUtility"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="Save{T}"/> always writes to the filesystem.
	/// </para>
	/// <para>
	/// <see cref="Load{T}"/> and <see cref="TryLoad{T}"/> read using <see cref="PathUtils.ReadTextFromPath"/>,
	/// so they can resolve absolute paths, Resources paths, or StreamingAssets-relative paths.
	/// </para>
	/// </remarks>
	public static class JsonIO
	{
		/// <summary>
		/// Serializes <paramref name="data"/> to JSON and writes it to the specified filesystem path.
		/// </summary>
		/// <typeparam name="T">Serializable data type.</typeparam>
		/// <param name="path">Filesystem path to write to.</param>
		/// <param name="data">Data to serialize.</param>
		/// <param name="prettyPrint">Whether to pretty-print the JSON.</param>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
		public static void Save<T>(string path, T data, bool prettyPrint = true)
		{
			// Validate output path early.
			if (string.IsNullOrEmpty(path))
			{
				throw new System.ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			// Ensure the destination directory exists.
			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Serialize then write to disk.
			string json = JsonUtility.ToJson(data, prettyPrint);
			File.WriteAllText(path, json);
		}

		/// <summary>
		/// Loads and deserializes JSON into <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Target data type.</typeparam>
		/// <param name="path">Path resolved via <see cref="PathUtils.ReadTextFromPath"/>.</param>
		/// <returns>
		/// Deserialized instance, or <c>default</c> if the path is missing or invalid.
		/// </returns>
		public static T Load<T>(string path)
		{
			// Resolve JSON content from supported locations.
			string json = PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				return default;
			}

			// Deserialize into the requested type.
			return JsonUtility.FromJson<T>(json);
		}

		/// <summary>
		/// Attempts to load and deserialize JSON into <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Target data type (class).</typeparam>
		/// <param name="path">Path resolved via <see cref="PathUtils.ReadTextFromPath"/>.</param>
		/// <param name="data">Output data when successful; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if a valid JSON payload was loaded; otherwise <c>false</c>.</returns>
		public static bool TryLoad<T>(string path, out T data) where T : class
		{
			// Default output for failure cases.
			data = null;

			// Resolve JSON content from supported locations.
			string json = PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				return false;
			}

			// Deserialize and validate non-null result.
			data = JsonUtility.FromJson<T>(json);
			return data != null;
		}
	}
}
