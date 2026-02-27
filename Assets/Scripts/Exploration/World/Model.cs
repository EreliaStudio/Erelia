using System.Collections.Generic;

namespace Erelia.World
{
	public sealed class Model
	{
		private readonly Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model> chunks = new Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model>();

		public IReadOnlyDictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model> Chunks => chunks;

		public Model()
		{
		}

		public Erelia.World.Chunk.Model GetOrCreateChunk(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (chunks.TryGetValue(coordinates, out Erelia.World.Chunk.Model existing))
			{
				return existing;
			}

			var chunk = new Erelia.World.Chunk.Model();
			chunks.Add(coordinates, chunk);
			return chunk;
		}
	}
}
