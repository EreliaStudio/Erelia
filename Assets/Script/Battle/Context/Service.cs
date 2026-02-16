namespace Battle.Context
{
	public class Service
	{
		private Model.TeamPlacement playerPlacement;
		public Model.TeamPlacement PlayerPlacement => playerPlacement;

		public void InitializeFromPlayerTeam(Core.Creature.Model.Team team)
		{
			playerPlacement = new Model.TeamPlacement(team, Model.Side.Player);
		}
	}
}
