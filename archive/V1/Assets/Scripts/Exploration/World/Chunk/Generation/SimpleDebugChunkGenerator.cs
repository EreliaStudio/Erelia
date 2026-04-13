using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World.Chunk.Generation
{
	[CreateAssetMenu(menuName = "World/Chunk Generator/Simple Debug", fileName = "SimpleDebugChunkGenerator")]
	public sealed class SimpleDebugChunkGenerator : IGenerator
	{
		[SerializeField] private Erelia.Exploration.World.BiomeRegistry biomeRegistry;

		private void OnEnable()
		{
			if (biomeRegistry == null)
			{
				biomeRegistry = Erelia.Exploration.World.BiomeRegistry.Instance;
				if (biomeRegistry == null)
				{
					Debug.LogWarning("[SimpleDebugChunkGenerator] BiomeRegistry is missing.");
				}
			}
		}

		public override void Generate(Erelia.Exploration.World.Chunk.ChunkData chunk, Erelia.Exploration.World.Chunk.Coordinates coordinates, Erelia.Exploration.World.WorldState worldModel)
		{
			if (chunk == null)
			{
				return;
			}

			int maxX = Erelia.Exploration.World.Chunk.ChunkData.SizeX - 1;
			int maxY = Erelia.Exploration.World.Chunk.ChunkData.SizeY - 1;
			int maxZ = Erelia.Exploration.World.Chunk.ChunkData.SizeZ - 1;

			if (maxX < 0 || maxY < 0 || maxZ < 0)
			{
				return;
			}

			for (int x = 0; x <= maxX; x++)
			{
				for (int z = 0; z <= maxZ; z++)
				{
					chunk.SetCell(x, 0, z, new Erelia.Core.Voxel.Cell(0));
				}
			}

			int encounterId = Erelia.Exploration.World.Chunk.ChunkData.NoEncounterId;
			if (biomeRegistry.Entries.Count > 0)
			{
				Erelia.Exploration.World.Biome biome = biomeRegistry.Entries[0].Biome;
				if (biome != null)
				{
					encounterId = biome.EncounterId;
				}
			}

			if (maxY < 1)
			{
				return;
			}

			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(0, 1, z, new Erelia.Core.Voxel.Cell(1));
			}

			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(maxX, 1, z, new Erelia.Core.Voxel.Cell(2));
			}

			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 0, new Erelia.Core.Voxel.Cell(3));
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, maxZ, new Erelia.Core.Voxel.Cell(4));
				chunk.SetEncounterId(x, 1, maxZ, encounterId);
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 5, new Erelia.Core.Voxel.Cell(4));
				chunk.SetEncounterId(x, 1, 5, encounterId);
			}
		}

		public override void Save(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(path, "{}");
		}

		public override void Load(string path)
		{
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

