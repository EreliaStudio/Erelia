namespace Erelia.Exploration.World
{
	/// <summary>
	/// Data container for biome-specific configuration.
	/// Holds encounter configuration used by generation and exploration triggers.
	/// </summary>
	[System.Serializable]
	public sealed class BiomeData
	{
		/// <summary>
		/// Encounter table id associated with this biome.
		/// </summary>
		public int EncounterId = Erelia.Exploration.World.Chunk.Model.NoEncounterId;
	}
}
