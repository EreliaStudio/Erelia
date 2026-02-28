namespace Erelia
{
	public sealed class Context
	{
		private static Context instance;

		public static Context Instance => instance ??= new Context();

		public Erelia.Exploration.Data ExplorationData { get; private set; }
		public Erelia.Battle.Data BattleData { get; private set; }

		private Context()
		{
			ExplorationData = new Erelia.Exploration.Data();
		}

		public void SetExploration(Erelia.Exploration.World.Model worldModel, Erelia.Exploration.Player.Model playerModel)
		{
			if (worldModel == null)
			{
				throw new System.ArgumentNullException(nameof(worldModel), "World model cannot be null.");
			}

			if (playerModel == null)
			{
				throw new System.ArgumentNullException(nameof(playerModel), "Player model cannot be null.");
			}

			ExplorationData = new Erelia.Exploration.Data
			{
				WorldModel = worldModel,
				PlayerModel = playerModel
			};
		}

		public void SetBattle(Erelia.Encounter.EncounterTable encounterTable, Erelia.Battle.Board.Model battleBoard)
		{
			Erelia.Battle.Data data = GetOrCreateBattleData();
			data.EncounterTable = encounterTable;
			data.Board = battleBoard;
		}

		public void ClearBattle()
		{
			BattleData = null;
		}

		public Erelia.Battle.Data GetOrCreateBattleData()
		{
			if (BattleData == null)
			{
				BattleData = new Erelia.Battle.Data();
			}

			return BattleData;
		}
	}
}
