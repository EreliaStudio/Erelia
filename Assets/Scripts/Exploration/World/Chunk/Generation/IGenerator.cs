using UnityEngine;

namespace Erelia.Exploration.World.Chunk.Generation
{
	public abstract class IGenerator : ScriptableObject
	{
		public abstract void Generate(Erelia.Exploration.World.Chunk.ChunkData chunk, Erelia.Exploration.World.Chunk.Coordinates coordinates, Erelia.Exploration.World.WorldState worldModel);

		public abstract void Save(string path);

		public abstract void Load(string path);
	}
}
