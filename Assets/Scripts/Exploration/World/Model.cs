using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World
{
	public sealed class Model
	{
		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> chunks = new Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model>();
		private string chunkDirectory;
		private Erelia.Exploration.World.Chunk.Generation.IGenerator chunkGenerator;
		private string generatorType;
		private string generatorDataPath;

		public IReadOnlyDictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> Chunks => chunks;
		public string ChunkDirectory => chunkDirectory;
		public bool HasChunkGenerator => chunkGenerator != null;

		public Model()
		{
		}

		public void SetChunkDirectory(string path)
		{
			chunkDirectory = path;
			if (!string.IsNullOrEmpty(chunkDirectory))
			{
				Directory.CreateDirectory(chunkDirectory);
			}
		}

		public void SetChunkGenerator(Erelia.Exploration.World.Chunk.Generation.IGenerator generator)
		{
			chunkGenerator = generator;
			generatorType = generator != null ? generator.GetType().AssemblyQualifiedName : null;
		}

		public Erelia.Exploration.World.Chunk.Model GetOrCreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (chunks.TryGetValue(coordinates, out Erelia.Exploration.World.Chunk.Model existing))
			{
				return existing;
			}

			var chunk = new Erelia.Exploration.World.Chunk.Model();
			string path = GetChunkPath(coordinates);
			bool loaded = false;
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				loaded = chunk.Load(path);
			}

			if (!loaded && chunkGenerator != null)
			{
				chunkGenerator.Generate(chunk, coordinates, this);
			}

			chunks.Add(coordinates, chunk);
			return chunk;
		}

		public void Clear()
		{
			chunks.Clear();
		}

		public void SaveChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (coordinates == null)
			{
				throw new System.ArgumentNullException(nameof(coordinates));
			}

			if (!chunks.TryGetValue(coordinates, out Erelia.Exploration.World.Chunk.Model chunk) || chunk == null)
			{
				return;
			}

			string path = GetChunkPath(coordinates);
			if (string.IsNullOrEmpty(path))
			{
				throw new System.InvalidOperationException("[Erelia.Exploration.World.Model] Chunk directory is not set.");
			}

			chunk.Save(path);
		}

		public void Save(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			string baseDirectory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(baseDirectory))
			{
				Directory.CreateDirectory(baseDirectory);
			}

			string resolvedChunkDir = ResolveChunkDirectory(baseDirectory);
			if (!string.IsNullOrEmpty(resolvedChunkDir))
			{
				Directory.CreateDirectory(resolvedChunkDir);
			}

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

			var data = new WorldSaveData
			{
				ChunkDirectory = MakeRelativePath(baseDirectory, resolvedChunkDir),
				GeneratorType = generatorType,
				GeneratorDataPath = generatorDataPath
			};

			string json = JsonUtility.ToJson(data, true);
			File.WriteAllText(path, json);
		}

		public bool Load(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				Debug.LogWarning($"[Erelia.Exploration.World.Model] Save file not found at '{path}'.");
				return false;
			}

			WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);
			if (data == null)
			{
				Debug.LogWarning("[Erelia.Exploration.World.Model] Failed to parse world save data.");
				return false;
			}

			string baseDirectory = Path.GetDirectoryName(path);
			string resolvedChunkDir = ResolvePath(baseDirectory, data.ChunkDirectory);
			SetChunkDirectory(resolvedChunkDir);

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

			chunks.Clear();
			return true;
		}

		public void SaveAllChunks()
		{
			if (string.IsNullOrEmpty(chunkDirectory))
			{
				throw new System.InvalidOperationException("[Erelia.Exploration.World.Model] Chunk directory is not set.");
			}

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

				entry.Value.Save(path);
			}
		}

		public void LoadChunkDirectory(string chunkFolderPath)
		{
			SetChunkDirectory(chunkFolderPath);
			chunks.Clear();
		}

		private string GetChunkPath(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (coordinates == null || string.IsNullOrEmpty(chunkDirectory))
			{
				return null;
			}

			string filename = $"chunk_{coordinates.X}_{coordinates.Z}.bin";
			string resolved = ResolvePath(null, chunkDirectory);
			return string.IsNullOrEmpty(resolved) ? null : Path.Combine(resolved, filename);
		}

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

		private static string MakeRelativePath(string baseDirectory, string path)
		{
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(baseDirectory))
			{
				return path;
			}

			try
			{
				Uri baseUri = new Uri(AppendDirectorySeparator(baseDirectory));
				Uri pathUri = new Uri(path);
				if (baseUri.IsBaseOf(pathUri))
				{
					return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
				}
			}
			catch (UriFormatException)
			{
				return path;
			}

			return path;
		}

		private static string AppendDirectorySeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			char separator = Path.DirectorySeparatorChar;
			return path.EndsWith(separator) ? path : path + separator;
		}

		[System.Serializable]
		private sealed class WorldSaveData
		{
			public string ChunkDirectory;
			public string GeneratorType;
			public string GeneratorDataPath;
		}
	}
}
