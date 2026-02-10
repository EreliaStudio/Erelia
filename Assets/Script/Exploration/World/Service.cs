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

		public void Init()
		{
			if (generator == null)
			{
				Debug.LogError("World.Service Init failed: generator is not assigned.");
				return;
			}

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

			//We need to get the content of the cells in world space within center - size/2 and center + size/2. For this, i need to 
		
			return result;
		}
	}
}
