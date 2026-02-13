namespace Battle.Phase
{
	public abstract class AbstractPhase
	{
		protected Battle.Phase.Manager manager;
		public Battle.Phase.Manager Manager => manager;

		public void SetManager(Battle.Phase.Manager manager)
		{
			this.manager = manager;
		}

		abstract public void OnEnter();
		abstract public void OnUpdate();
		abstract public void OnExit();
	}
}