namespace Battle.Phase
{
	public abstract class AbstractPhase
	{
		protected Battle.Phase.Manager manager;
		public Battle.Phase.Manager Manager => manager;
		protected Battle.Player.Controller.BattleController playerController;
		public Battle.Player.Controller.BattleController PlayerController => playerController;

		public void Setup(Battle.Phase.Manager manager, Battle.Player.Controller.BattleController playerController)
		{
			this.manager = manager;
			this.playerController = playerController;
		}

		abstract public void OnEnter();
		abstract public void OnUpdate();
		abstract public void OnExit();
	}
}