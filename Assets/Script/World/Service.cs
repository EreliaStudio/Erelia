using UnityEngine;
using System;
using System.Collections.Generic;

namespace World
{
	[Serializable]
	public class Service
	{	
		[SerializeField] private World.Chunk.IGenerator generator = null;
		
		private Dictionary<World.Chunk.Coordinates, World.Chunk.Data> chunks = new Dictionary<World.Chunk.Coordinates, World.Chunk.Data>();

		public void Init()
		{
			if (generator == null)
			{
				Debug.LogError("World.Service Init failed: generator is not assigned.");
				return;
			}

			chunks.Clear();
		}

		public Chunk.Data GetOrCreateChunk(Chunk.Coordinates coord)
		{
			if (generator == null)
			{
				Debug.LogError("World.Service is not initialized. Call Init() before use.");
				return null;
			}

			if (!chunks.TryGetValue(coord, out Chunk.Data chunk))
			{
				chunk = generator.Generate(coord);
				chunks.Add(coord, chunk);
			}

			return chunk;
		}
	}
}
