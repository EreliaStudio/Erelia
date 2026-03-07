namespace Erelia.Battle
{
	/// <summary>
	/// Serializable container for battle runtime data.
	/// Stores the board and encounter table for the current battle.
	/// </summary>
	[System.Serializable]
	public sealed class Data
	{
		/// <summary>
		/// Battle board model for the current encounter.
		/// </summary>
		public Erelia.Battle.Board.Model Board;
		/// <summary>
		/// Encounter table used to configure the battle.
		/// </summary>
		public Erelia.Core.Encounter.EncounterTable EncounterTable;
		/// <summary>
		/// Shared phase-specific runtime info built as the battle progresses.
		/// </summary>
		public Erelia.Battle.Phase.Info PhaseInfo;

		/// <summary>
		/// Creates an empty battle data container with default info.
		/// </summary>
		public Data()
		{
			// Initialize defaults for battle state.
			Board = null;
			EncounterTable = null;
			PhaseInfo = null;
		}
	}
}
