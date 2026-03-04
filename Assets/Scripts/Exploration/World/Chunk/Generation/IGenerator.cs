using UnityEngine;

namespace Erelia.Exploration.World.Chunk.Generation
{
	/// <summary>
	/// Base class for chunk generators.
	/// Defines the Generate/Save/Load contract for procedural chunk generation.
	/// </summary>
	public abstract class IGenerator : ScriptableObject
	{
		/// <summary>
		/// Populates the provided chunk model with voxel data.
		/// </summary>
		/// <param name="chunk">Chunk model to populate.</param>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <param name="worldModel">Owning world model.</param>
		public abstract void Generate(Erelia.Exploration.World.Chunk.Model chunk, Erelia.Exploration.World.Chunk.Coordinates coordinates, Erelia.Exploration.World.Model worldModel);

		/// <summary>
		/// Saves generator state to a file.
		/// </summary>
		/// <param name="path">Filesystem path to write to.</param>
		public abstract void Save(string path);

		/// <summary>
		/// Loads generator state from a file.
		/// </summary>
		/// <param name="path">Filesystem path to read from.</param>
		public abstract void Load(string path);
	}
}
