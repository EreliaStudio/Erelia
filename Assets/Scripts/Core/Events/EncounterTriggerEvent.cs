namespace Erelia.Core.Event
{
	/// <summary>
	/// Event emitted when an encounter is triggered.
	/// </summary>
	/// <remarks>
	/// Carries the resolved enemy team and the prebuilt battle board model.
	/// </remarks>
	public sealed class EncounterTriggerEvent : GenericEvent
	{
		/// <summary>
		/// Enemy team selected for this encounter.
		/// </summary>
		public Erelia.Core.Creature.Team EnemyTeam { get; }

		/// <summary>
		/// Battle board model generated for this encounter.
		/// </summary>
		public Erelia.Battle.Board.Model BattleBoard { get; }

		/// <summary>
		/// Creates a new encounter trigger event.
		/// </summary>
		/// <param name="enemyTeam">Enemy team selected for this encounter.</param>
		/// <param name="battleBoard">Battle board model to use for this encounter.</param>
		public EncounterTriggerEvent(
			Erelia.Core.Creature.Team enemyTeam,
			Erelia.Battle.Board.Model battleBoard)
		{
			EnemyTeam = enemyTeam;
			BattleBoard = battleBoard;
		}
	}
}
