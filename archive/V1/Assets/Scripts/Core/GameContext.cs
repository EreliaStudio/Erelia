namespace Erelia.Core
{
	public sealed class GameContext
	{
		private static GameContext instance = null;

		public static GameContext Instance => instance ??= new GameContext();

		public Erelia.Exploration.ExplorationState Exploration { get; private set; } = new Erelia.Exploration.ExplorationState();
		public Erelia.Battle.BattleState Battle { get; private set; } = new Erelia.Battle.BattleState();
		public Erelia.Core.PlayerPartyState PlayerParty { get; private set; } = new Erelia.Core.PlayerPartyState();

		public void SetExploration(Erelia.Exploration.World.WorldState worldState, Erelia.Exploration.Player.ExplorationPlayerState playerState)
		{
			Exploration.World = worldState;
			Exploration.Player = playerState;
		}

		public void SetBattle(Erelia.Core.Creature.Team enemyTeam, Erelia.Battle.Board.BattleBoardState battleBoard)
		{
			Battle.Reset(enemyTeam, battleBoard);
		}
	}
}


