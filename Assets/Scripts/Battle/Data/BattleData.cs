namespace Erelia.Battle
{
	/// <summary>
	/// Serializable container for battle runtime data.
	/// Stores the board and enemy team for the current battle.
	/// </summary>
	[System.Serializable]
	public sealed class Data
	{
		/// <summary>
		/// Battle board model for the current encounter.
		/// </summary>
		public Erelia.Battle.Board.Model Board;
		/// <summary>
		/// Enemy team to fight in the current battle.
		/// </summary>
		public Erelia.Core.Creature.Team EnemyTeam;
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
			EnemyTeam = null;
			PhaseInfo = null;
		}
	}
}
