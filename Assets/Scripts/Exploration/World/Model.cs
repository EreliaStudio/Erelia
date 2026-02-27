using System.Collections.Generic;

namespace Erelia.Exploration.World
{
	public sealed class Model
	{
		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> chunks = new Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model>();

		public IReadOnlyDictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Model> Chunks => chunks;

		public Model()
		{
		}

		public Erelia.Exploration.World.Chunk.Model GetOrCreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (chunks.TryGetValue(coordinates, out Erelia.Exploration.World.Chunk.Model existing))
			{
				return existing;
			}

			var chunk = new Erelia.Exploration.World.Chunk.Model();
			chunks.Add(coordinates, chunk);
			return chunk;
		}
	}
}
