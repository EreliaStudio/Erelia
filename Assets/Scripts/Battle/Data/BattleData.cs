namespace Erelia.Battle
{
	/// <summary>
	/// Serializable container for battle runtime data.
	/// Stores the board, encounter table, and computed battle info.
	/// </summary>
	[System.Serializable]
	public sealed class Data
	{
		/// <summary>
		/// Battle board model for the current encounter.
		/// </summary>
		public Erelia.Battle.Board.Model Board;
		/// <summary>
		/// Derived battle info (placement centers, etc.).
		/// </summary>
		public Erelia.Battle.Info Info;
		/// <summary>
		/// Encounter table used to configure the battle.
		/// </summary>
		public Erelia.Core.Encounter.EncounterTable EncounterTable;

		/// <summary>
		/// Creates an empty battle data container with default info.
		/// </summary>
		public Data()
		{
			// Initialize defaults for battle state.
			Board = null;
			EncounterTable = null;
			Info = new Erelia.Battle.Info();
		}
	}
}
