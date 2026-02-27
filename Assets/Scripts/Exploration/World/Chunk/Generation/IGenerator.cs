using UnityEngine;

namespace Erelia.World.Chunk.Generation
{
	public abstract class IGenerator : ScriptableObject
	{
		public abstract void Generate(Erelia.World.Chunk.Model chunk, Erelia.World.Chunk.Coordinates coordinates, Erelia.World.Model worldModel);
	}
}
