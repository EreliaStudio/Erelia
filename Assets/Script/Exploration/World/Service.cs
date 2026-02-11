using UnityEngine;
using System;
using System.Collections.Generic;

namespace World
{
	[Serializable]
	public class Service
	{	
		[SerializeField] private World.Chunk.Model.IGenerator generator = null;
		
		private Dictionary<World.Chunk.Model.Coordinates, World.Chunk.Model.Data> chunks = new Dictionary<World.Chunk.Model.Coordinates, World.Chunk.Model.Data>();

		public Service(Chunk.Model.IGenerator generator)
		{
			this.generator = generator;
			chunks.Clear();
		}

		public Chunk.Model.Data GetOrCreateChunk(Chunk.Model.Coordinates coord)
		{
			if (generator == null)
			{
				Debug.LogError("World.Service is not initialized. Call Init() before use.");
				return null;
			}

			if (!chunks.TryGetValue(coord, out Chunk.Model.Data chunk))
			{
				chunk = generator.Generate(coord);
				chunks.Add(coord, chunk);
			}

			return chunk;
		}

		public Voxel.Model.Cell[,,] ExtrudeCells(Vector2Int center, Vector2Int size)
		{
			Voxel.Model.Cell[,,] result = new Voxel.Model.Cell[size.x, World.Chunk.Model.Data.SizeY, size.y];
			int startX = center.x - (size.x / 2);
			int startZ = center.y - (size.y / 2);

			for (int x = 0; x < size.x; x++)
			{
				int worldX = startX + x;
				for (int z = 0; z < size.y; z++)
				{
					int worldZ = startZ + z;
					for (int y = 0; y < World.Chunk.Model.Data.SizeY; y++)
					{
						result[x, y, z] = GetCellAtWorld(worldX, y, worldZ);
					}
				}
			}

			return result;
		}

		private Voxel.Model.Cell GetCellAtWorld(int worldX, int worldY, int worldZ)
		{
			int chunkX = Mathf.FloorToInt((float)worldX / World.Chunk.Model.Data.SizeX);
			int chunkY = Mathf.FloorToInt((float)worldY / World.Chunk.Model.Data.SizeY);
			int chunkZ = Mathf.FloorToInt((float)worldZ / World.Chunk.Model.Data.SizeZ);

			int localX = worldX - (chunkX * World.Chunk.Model.Data.SizeX);
			int localY = worldY - (chunkY * World.Chunk.Model.Data.SizeY);
			int localZ = worldZ - (chunkZ * World.Chunk.Model.Data.SizeZ);

			World.Chunk.Model.Data chunk = GetOrCreateChunk(new World.Chunk.Model.Coordinates(chunkX, chunkY, chunkZ));
			if (chunk == null)
			{
				return new Voxel.Model.Cell(-1);
			}

			return chunk.Cells[localX, localY, localZ];
		}
	}
}
