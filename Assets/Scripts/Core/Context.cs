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

		public void SetBattle(Erelia.Core.Encounter.EncounterTable encounterTable, Erelia.Battle.Board.Model battleBoard)
		{
			BattleData.EncounterTable = encounterTable;
			BattleData.Board = battleBoard;
			BattleData.PhaseInfo = new Battle.Phase.Info();
		}
	}
}
