namespace Erelia.Core.Event
{
	/// <summary>
	/// Event requesting that the battle scene be loaded with prepared battle data.
	/// </summary>
	/// <remarks>
	/// Carries the prebuilt battle board and resolved enemy team for the upcoming battle.
	/// </remarks>
	public sealed class BattleSceneDataRequest : GenericEvent
	{
		/// <summary>
		/// Enemy team to fight in the battle scene.
		/// </summary>
		public Erelia.Core.Creature.Team EnemyTeam { get; }

		/// <summary>
		/// Battle board model to use for the battle scene.
		/// </summary>
		public Erelia.Battle.Board.Model BattleBoard { get; }

		/// <summary>
		/// Creates a new battle scene data request.
		/// </summary>
		/// <param name="enemyTeam">Enemy team to fight.</param>
		/// <param name="battleBoard">Battle board to use.</param>
		public BattleSceneDataRequest(
			Erelia.Core.Creature.Team enemyTeam,
			Erelia.Battle.Board.Model battleBoard)
		{
			EnemyTeam = enemyTeam;
			BattleBoard = battleBoard;
		}
	}
}
