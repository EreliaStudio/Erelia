using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World.Chunk.Generation
{
	/// <summary>
	/// Simple debug chunk generator that builds a bordered floor and encounter line.
	/// Generates a floor and walls, then stamps encounter ids using the first biome entry.
	/// </summary>
	[CreateAssetMenu(menuName = "World/Chunk Generator/Simple Debug", fileName = "SimpleDebugChunkGenerator")]
	public sealed class SimpleDebugChunkGenerator : IGenerator
	{
		/// <summary>
		/// Biome registry used to resolve encounter ids.
		/// </summary>
		[SerializeField] private Erelia.Exploration.World.BiomeRegistry biomeRegistry;

		/// <summary>
		/// Unity callback invoked when the asset is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Resolve registry if not assigned.
			if (biomeRegistry == null)
			{
				biomeRegistry = Erelia.Exploration.World.BiomeRegistry.Instance;
				if (biomeRegistry == null)
				{
					Debug.LogWarning("[SimpleDebugChunkGenerator] BiomeRegistry is missing.");
				}
			}
		}

		/// <summary>
		/// Generates a simple debug chunk layout.
		/// </summary>
		/// <param name="chunk">Chunk to populate.</param>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <param name="worldModel">Owning world model.</param>
		public override void Generate(Erelia.Exploration.World.Chunk.Model chunk, Erelia.Exploration.World.Chunk.Coordinates coordinates, Erelia.Exploration.World.Model worldModel)
		{
			// Validate input.
			if (chunk == null)
			{
				return;
			}

			// Compute bounds.
			int maxX = Erelia.Exploration.World.Chunk.Model.SizeX - 1;
			int maxY = Erelia.Exploration.World.Chunk.Model.SizeY - 1;
			int maxZ = Erelia.Exploration.World.Chunk.Model.SizeZ - 1;

			if (maxX < 0 || maxY < 0 || maxZ < 0)
			{
				return;
			}

			// Fill the ground layer.
			for (int x = 0; x <= maxX; x++)
			{
				for (int z = 0; z <= maxZ; z++)
				{
					chunk.SetCell(x, 0, z, new Erelia.Core.VoxelKit.Cell(0));
				}
			}

			// Resolve encounter id from the first biome entry if available.
			int encounterId = Erelia.Exploration.World.Chunk.Model.NoEncounterId;
			if (biomeRegistry.Entries.Count > 0)
			{
				Erelia.Exploration.World.BiomeData data = biomeRegistry.Entries[0].Data;
				if (data != null)
				{
					encounterId = data.EncounterId;
				}
			}

			// No vertical layers to build.
			if (maxY < 1)
			{
				return;
			}

			// Place simple walls and encounter strip.
			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(0, 1, z, new Erelia.Core.VoxelKit.Cell(1));
			}

			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(maxX, 1, z, new Erelia.Core.VoxelKit.Cell(2));
			}

			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 0, new Erelia.Core.VoxelKit.Cell(3));
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, maxZ, new Erelia.Core.VoxelKit.Cell(4));
				chunk.SetEncounterId(x, 1, maxZ, encounterId);
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 5, new Erelia.Core.VoxelKit.Cell(4));
				chunk.SetEncounterId(x, 1, 5, encounterId);
			}
		}

		/// <summary>
		/// Saves generator state to disk.
		/// </summary>
		/// <param name="path">Filesystem path to write to.</param>
		public override void Save(string path)
		{
			// Ignore invalid paths.
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			// Ensure destination directory exists.
			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// This generator has no state, so write an empty JSON object.
			File.WriteAllText(path, "{}");
		}

		/// <summary>
		/// Loads generator state from disk.
		/// </summary>
		/// <param name="path">Filesystem path to read from.</param>
		public override void Load(string path)
		{
			// Ensure biome registry is available.
			if (biomeRegistry == null)
			{
				biomeRegistry = Erelia.Exploration.World.BiomeRegistry.Instance;
				if (biomeRegistry == null)
				{
					Debug.LogWarning("[SimpleDebugChunkGenerator] BiomeRegistry is missing.");
				}
			}
		}

	}
}
