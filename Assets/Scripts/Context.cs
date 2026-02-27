namespace Erelia
{
	public sealed class Context
	{
		private static Context instance;

		public static Context Instance => instance ??= new Context();

		public Erelia.Exploration.World.Model WorldModel { get; private set; }
		public Erelia.Exploration.Player.Model PlayerModel { get; private set; }
		public Erelia.Battle.Board.Model PendingBattleBoard { get; private set; }
		public Erelia.Encounter.EncounterTable PendingEncounterTable { get; private set; }

		private Context()
		{
			WorldModel = new Erelia.Exploration.World.Model();
			PlayerModel = new Erelia.Exploration.Player.Model();
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

			WorldModel = worldModel;
			PlayerModel = playerModel;
		}

		public void SetBattle(Erelia.Encounter.EncounterTable encounterTable, Erelia.Battle.Board.Model battleBoard)
		{
			PendingEncounterTable = encounterTable;
			PendingBattleBoard = battleBoard;
		}

		public void ClearBattle()
		{
			PendingEncounterTable = null;
			PendingBattleBoard = null;
		}
	}
}
