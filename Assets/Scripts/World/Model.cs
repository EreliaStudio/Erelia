using System.Collections.Generic;

namespace Erelia.World
{
	public sealed class Model
	{
		private readonly Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model> chunks = new Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model>();

		public IReadOnlyDictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Model> Chunks => chunks;
		public Erelia.World.BiomeRegistry BiomeRegistry { get; }
		public Erelia.Encounter.EncounterTableRegistry EncounterTableRegistry { get; }

		public Model(Erelia.World.BiomeRegistry biomeRegistry = null, Erelia.Encounter.EncounterTableRegistry encounterTableRegistry = null)
		{
			BiomeRegistry = biomeRegistry ?? Erelia.World.BiomeRegistry.Instance;

			EncounterTableRegistry = encounterTableRegistry;
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
