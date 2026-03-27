namespace Erelia.Core.Event
{
	public sealed class BattleSceneDataRequest : GenericEvent
	{
		public Erelia.Core.Creature.Team EnemyTeam { get; }

		public Erelia.Battle.Board.BattleBoardState BattleBoard { get; }

		public BattleSceneDataRequest(
			Erelia.Core.Creature.Team enemyTeam,
			Erelia.Battle.Board.BattleBoardState battleBoard)
		{
			EnemyTeam = enemyTeam;
			BattleBoard = battleBoard;
		}
	}
}
