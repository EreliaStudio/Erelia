namespace Erelia.Core
{
	public sealed class Context
	{
		private static Context instance = null;

		public static Context Instance => instance ??= new Context();

		public Erelia.Exploration.Data ExplorationData { get; private set; } = new Erelia.Exploration.Data();
		public Erelia.Battle.Data BattleData { get; private set; } = new Erelia.Battle.Data();
		public Erelia.Core.SystemData SystemData { get; private set; } = new Erelia.Core.SystemData();

		public void SetExploration(Erelia.Exploration.World.Model worldModel, Erelia.Exploration.Player.Model playerModel)
		{
			ExplorationData.WorldModel = worldModel;
			ExplorationData.PlayerModel = playerModel;
		}

		public void SetBattle(Erelia.Core.Creature.Team enemyTeam, Erelia.Battle.Board.Model battleBoard)
		{
			BattleData.EnemyTeam = enemyTeam;
			BattleData.Board = battleBoard;
			BattleData.PhaseInfo = new Battle.Phase.Info();
			BattleData.Timeline = null;
			BattleData.ActiveUnit = null;
		}
	}
}
