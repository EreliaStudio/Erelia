using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World
{
	/// <summary>
	/// Data model for the exploration world, including chunk storage and generation.
	/// Caches chunks, loads or generates them on demand, and persists world metadata and generator state.
	/// </summary>
	/// <remarks>
	/// Handles chunk caching, chunk file paths, and generator serialization.
	/// </remarks>
	public sealed class Model
	{
		/// <summary>
		/// In-memory chunk cache keyed by chunk coordinates.
		/// </summary>
		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> chunks = new Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model>();

		/// <summary>
		/// Directory used to store chunk files.
		/// </summary>
		private string chunkDirectory;

		/// <summary>
		/// Generator used to populate new chunks.
		/// </summary>
		private Erelia.Exploration.World.Chunk.Generation.IGenerator chunkGenerator;

		/// <summary>
		/// Assembly-qualified generator type name (for save/load).
		/// </summary>
		private string generatorType;

		/// <summary>
		/// Path to the generator data file (for save/load).
		/// </summary>
		private string generatorDataPath;

		/// <summary>
		/// Gets the chunk cache.
		/// </summary>
		public IReadOnlyDictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> Chunks => chunks;

		/// <summary>
		/// Gets the current chunk directory.
		/// </summary>
		public string ChunkDirectory => chunkDirectory;

		/// <summary>
		/// Gets whether a chunk generator is assigned.
		/// </summary>
		public bool HasChunkGenerator => chunkGenerator != null;

		/// <summary>
		/// Creates a new world model.
		/// </summary>
		public Model()
		{
		}

		/// <summary>
		/// Sets the chunk directory and ensures it exists.
		/// </summary>
		/// <param name="path">Directory path to use.</param>
		public void SetChunkDirectory(string path)
		{
			// Store directory and ensure it exists.
			chunkDirectory = path;
			if (!string.IsNullOrEmpty(chunkDirectory))
			{
				Directory.CreateDirectory(chunkDirectory);
			}
		}

		/// <summary>
		/// Assigns the chunk generator.
		/// </summary>
		/// <param name="generator">Generator to use for new chunks.</param>
		public void SetChunkGenerator(Erelia.Exploration.World.Chunk.Generation.IGenerator generator)
		{
			// Store generator and cache its type for saving.
			chunkGenerator = generator;
			generatorType = generator != null ? generator.GetType().AssemblyQualifiedName : null;
		}

		/// <summary>
		/// Returns an existing chunk or creates/loads one if missing.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns>Chunk model instance.</returns>
		public Erelia.Exploration.World.Chunk.Model GetOrCreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			// Return cached chunk if available.
			if (chunks.TryGetValue(coordinates, out Erelia.Exploration.World.Chunk.Model existing))
			{
				return existing;
			}

			// Create and attempt to load a chunk from disk.
			var chunk = new Erelia.Exploration.World.Chunk.Model();
			string path = GetChunkPath(coordinates);
			bool loaded = false;
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				loaded = chunk.FromFile(path);
			}

			// If not loaded, generate procedurally.
			if (!loaded && chunkGenerator != null)
			{
				chunkGenerator.Generate(chunk, coordinates, this);
			}

			// Cache and return.
			chunks.Add(coordinates, chunk);
			return chunk;
		}

		/// <summary>
		/// Clears the in-memory chunk cache.
		/// </summary>
		public void Clear()
		{
			chunks.Clear();
		}

		/// <summary>
		/// Saves a single chunk to disk.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		public void SaveChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			// Validate input.
			if (coordinates == null)
			{
				throw new System.ArgumentNullException(nameof(coordinates));
			}

			// Skip missing chunks.
			if (!chunks.TryGetValue(coordinates, out Erelia.Exploration.World.Chunk.Model chunk) || chunk == null)
			{
				return;
			}

			// Resolve chunk path.
			string path = GetChunkPath(coordinates);
			if (string.IsNullOrEmpty(path))
			{
				throw new System.InvalidOperationException("[Erelia.Exploration.World.Model] Chunk directory is not set.");
			}

			// Write the chunk to disk.
			chunk.ToFile(path);
		}

		/// <summary>
		/// Saves the world metadata and generator state to a JSON file.
		/// </summary>
		/// <param name="path">Filesystem path to write to.</param>
		public void Save(string path)
		{
			// Validate output path.
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			// Ensure base directory exists.
			string baseDirectory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(baseDirectory))
			{
				Directory.CreateDirectory(baseDirectory);
			}

			// Resolve and ensure chunk directory exists.
			string resolvedChunkDir = ResolveChunkDirectory(baseDirectory);
			if (!string.IsNullOrEmpty(resolvedChunkDir))
			{
				Directory.CreateDirectory(resolvedChunkDir);
			}

			// Save generator state if available.
			if (chunkGenerator != null)
			{
				generatorType = chunkGenerator.GetType().AssemblyQualifiedName;
				string generatorFilename = Path.GetFileNameWithoutExtension(path) + "_generator.json";
				string generatorFullPath = string.IsNullOrEmpty(baseDirectory)
					? generatorFilename
					: Path.Combine(baseDirectory, generatorFilename);
				chunkGenerator.Save(generatorFullPath);
				generatorDataPath = MakeRelativePath(baseDirectory, generatorFullPath);
			}
			else
			{
				generatorType = null;
				generatorDataPath = null;
			}

			// Persist world metadata.
			var data = new WorldSaveData
			{
				ChunkDirectory = MakeRelativePath(baseDirectory, resolvedChunkDir),
				GeneratorType = generatorType,
				GeneratorDataPath = generatorDataPath
			};

			// Write JSON metadata.
			Erelia.Core.Utils.JsonIO.Save(path, data, true);
		}

		/// <summary>
		/// Loads world metadata and generator state from a JSON file.
		/// </summary>
		/// <param name="path">Path resolved via <see cref="Erelia.Core.Utils.PathUtils"/>.</param>
		/// <returns><c>true</c> if loaded successfully; otherwise <c>false</c>.</returns>
		public bool Load(string path)
		{
			// Validate input path.
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			// Read metadata JSON.
			WorldSaveData data = Erelia.Core.Utils.JsonIO.Load<WorldSaveData>(path);
			if (data == null)
			{
				Debug.LogWarning($"[Erelia.Exploration.World.Model] Save file not found or invalid at '{path}'.");
				return false;
			}

			// Resolve and store chunk directory.
			string baseDirectory = Path.GetDirectoryName(path);
			string resolvedChunkDir = ResolvePath(baseDirectory, data.ChunkDirectory);
			SetChunkDirectory(resolvedChunkDir);

			// Reset generator state then reload if configured.
			chunkGenerator = null;
			generatorType = data.GeneratorType;
			generatorDataPath = data.GeneratorDataPath;

			if (!string.IsNullOrEmpty(generatorType))
			{
				Type resolvedType = Type.GetType(generatorType);
				if (resolvedType == null || !typeof(Erelia.Exploration.World.Chunk.Generation.IGenerator).IsAssignableFrom(resolvedType))
				{
					Debug.LogWarning($"[Erelia.Exploration.World.Model] Generator type '{generatorType}' could not be resolved.");
				}
				else
				{
					// Instantiate generator and load its data.
					chunkGenerator = ScriptableObject.CreateInstance(resolvedType) as Erelia.Exploration.World.Chunk.Generation.IGenerator;
					if (chunkGenerator != null)
					{
						string generatorPath = ResolvePath(baseDirectory, generatorDataPath);
						if (!string.IsNullOrEmpty(generatorPath))
						{
							chunkGenerator.Load(generatorPath);
						}
					}
				}
			}

			// Clear cached chunks (they will be loaded lazily).
			chunks.Clear();
			return true;
		}

		/// <summary>
		/// Saves all loaded chunks to disk.
		/// </summary>
		public void SaveAllChunks()
		{
			// Ensure chunk directory is available.
			if (string.IsNullOrEmpty(chunkDirectory))
			{
				throw new System.InvalidOperationException("[Erelia.Exploration.World.Model] Chunk directory is not set.");
			}

			// Iterate and save each loaded chunk.
			foreach (KeyValuePair<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> entry in chunks)
			{
				if (entry.Key == null || entry.Value == null)
				{
					continue;
				}

				string path = GetChunkPath(entry.Key);
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}

				entry.Value.ToFile(path);
			}
		}

		/// <summary>
		/// Sets the chunk directory and clears cached chunks.
		/// </summary>
		/// <param name="chunkFolderPath">Directory path to use.</param>
		public void LoadChunkDirectory(string chunkFolderPath)
		{
			// Store new directory and reset cache.
			SetChunkDirectory(chunkFolderPath);
			chunks.Clear();
		}

		/// <summary>
		/// Builds a chunk file path for the given coordinates.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns>Chunk file path or <c>null</c> if unavailable.</returns>
		private string GetChunkPath(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (coordinates == null || string.IsNullOrEmpty(chunkDirectory))
			{
				return null;
			}

			// Use a consistent filename convention.
			string filename = $"chunk_{coordinates.X}_{coordinates.Z}.bin";
			string resolved = ResolvePath(null, chunkDirectory);
			return string.IsNullOrEmpty(resolved) ? null : Path.Combine(resolved, filename);
		}

		/// <summary>
		/// Resolves the final chunk directory based on the base directory and configuration.
		/// </summary>
		/// <param name="baseDirectory">Base path for relative resolution.</param>
		/// <returns>Resolved chunk directory path.</returns>
		private string ResolveChunkDirectory(string baseDirectory)
		{
			if (string.IsNullOrEmpty(chunkDirectory))
			{
				return string.IsNullOrEmpty(baseDirectory)
					? null
					: Path.Combine(baseDirectory, "chunks");
			}

			return ResolvePath(baseDirectory, chunkDirectory);
		}

		/// <summary>
		/// Resolves a path relative to a base directory when needed.
		/// </summary>
		/// <param name="baseDirectory">Base directory for relative paths.</param>
		/// <param name="path">Path to resolve.</param>
		/// <returns>Resolved absolute or relative path.</returns>
		private static string ResolvePath(string baseDirectory, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			if (Path.IsPathRooted(path) || string.IsNullOrEmpty(baseDirectory))
			{
				return path;
			}

			return Path.Combine(baseDirectory, path);
		}

		/// <summary>
		/// Converts an absolute path to a path relative to a base directory when possible.
		/// </summary>
		/// <param name="baseDirectory">Base directory.</param>
		/// <param name="path">Path to make relative.</param>
		/// <returns>Relative path if possible; otherwise original path.</returns>
		private static string MakeRelativePath(string baseDirectory, string path)
		{
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(baseDirectory))
			{
				return path;
			}

			try
			{
				// Build URIs for path math.
				Uri baseUri = new Uri(AppendDirectorySeparator(baseDirectory));
				Uri pathUri = new Uri(path);
				if (baseUri.IsBaseOf(pathUri))
				{
					return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
				}
			}
			catch (UriFormatException)
			{
				// Fallback to original path on invalid URIs.
				return path;
			}

			return path;
		}

		/// <summary>
		/// Ensures a path ends with a directory separator.
		/// </summary>
		/// <param name="path">Input path.</param>
		/// <returns>Path with trailing separator when needed.</returns>
		private static string AppendDirectorySeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			// Append separator if missing.
			char separator = Path.DirectorySeparatorChar;
			return path.EndsWith(separator) ? path : path + separator;
		}

		[System.Serializable]
		private sealed class WorldSaveData
		{
			/// <summary>
			/// Path to chunk directory (relative or absolute).
			/// </summary>
			public string ChunkDirectory;

			/// <summary>
			/// Assembly-qualified generator type name.
			/// </summary>
			public string GeneratorType;

			/// <summary>
			/// Path to generator data file (relative or absolute).
			/// </summary>
			public string GeneratorDataPath;
		}
	}
}
