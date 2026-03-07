namespace Erelia.Core
{
	public sealed class Context
	{
		private static Context instance;

		public static Context Instance => instance ??= new Context();

		public Erelia.Exploration.Data ExplorationData { get; private set; }
		public Erelia.Battle.Data BattleData { get; private set; }
		public Erelia.Core.SystemData SystemData { get; private set; }

		private Context()
		{
			ExplorationData = new Erelia.Exploration.Data();
			BattleData = new Erelia.Battle.Data();
			SystemData = new Erelia.Core.SystemData();
		}

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

		public void ClearBattle()
		{
			BattleData = null;
		}
	}
}
