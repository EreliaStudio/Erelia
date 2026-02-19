using UnityEngine;
using System;
using System.Collections.Generic;

namespace Exploration.World
{
	[Serializable]
	public class Service
	{	
		[SerializeField] private Exploration.World.Chunk.Model.AbstractGenerator generator = null;
		
		private Dictionary<Exploration.World.Chunk.Model.Coordinates, Exploration.World.Chunk.Model.Data> chunks = new Dictionary<Exploration.World.Chunk.Model.Coordinates, Exploration.World.Chunk.Model.Data>();

		public Service(Chunk.Model.AbstractGenerator generator)
		{
			this.generator = generator;
			chunks.Clear();
		}

		public Chunk.Model.Data GetOrCreateChunk(Chunk.Model.Coordinates coord)
		{
			if (generator == null)
			{
				Debug.LogError("Exploration.World.Service is not initialized. Call Init() before use.");
				return null;
			}

			if (!chunks.TryGetValue(coord, out Chunk.Model.Data chunk))
			{
				chunk = generator.Generate(coord);
				chunks.Add(coord, chunk);
			}

			return chunk;
		}

		public Core.Voxel.Model.Cell[,,] ExtrudeCells(Vector2Int center, Vector2Int size)
		{
			Core.Voxel.Model.Cell[,,] result = new Core.Voxel.Model.Cell[size.x, Exploration.World.Chunk.Model.Data.SizeY, size.y];
			int startX = center.x - (size.x / 2);
			int startZ = center.y - (size.y / 2);

			for (int x = 0; x < size.x; x++)
			{
				int worldX = startX + x;
				for (int z = 0; z < size.y; z++)
				{
					int worldZ = startZ + z;
					for (int y = 0; y < Exploration.World.Chunk.Model.Data.SizeY; y++)
					{
						result[x, y, z] = GetCellAtWorld(worldX, y, worldZ);
					}
				}
			}

			return result;
		}

		private Core.Voxel.Model.Cell GetCellAtWorld(int worldX, int worldY, int worldZ)
		{
			int chunkX = Mathf.FloorToInt((float)worldX / Exploration.World.Chunk.Model.Data.SizeX);
			int chunkY = Mathf.FloorToInt((float)worldY / Exploration.World.Chunk.Model.Data.SizeY);
			int chunkZ = Mathf.FloorToInt((float)worldZ / Exploration.World.Chunk.Model.Data.SizeZ);

			int localX = worldX - (chunkX * Exploration.World.Chunk.Model.Data.SizeX);
			int localY = worldY - (chunkY * Exploration.World.Chunk.Model.Data.SizeY);
			int localZ = worldZ - (chunkZ * Exploration.World.Chunk.Model.Data.SizeZ);

			Exploration.World.Chunk.Model.Data chunk = GetOrCreateChunk(new Exploration.World.Chunk.Model.Coordinates(chunkX, chunkY, chunkZ));
			if (chunk == null)
			{
				return new Core.Voxel.Model.Cell(-1);
			}

			return chunk.Cells[localX, localY, localZ];
		}
	}
}
