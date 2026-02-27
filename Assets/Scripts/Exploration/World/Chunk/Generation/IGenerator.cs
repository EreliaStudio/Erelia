using UnityEngine;

namespace Erelia.Exploration.World.Chunk.Generation
{
	public abstract class IGenerator : ScriptableObject
	{
		public abstract void Generate(Erelia.Exploration.World.Chunk.Model chunk, Erelia.Exploration.World.Chunk.Coordinates coordinates, Erelia.Exploration.World.Model worldModel);
	}
}
