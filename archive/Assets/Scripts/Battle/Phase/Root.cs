

namespace Erelia.Battle.Phase
{
	[System.Serializable]
	public abstract class Root : Controller
	{
		public abstract Id Id { get; }

		public virtual void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
		}

		public virtual void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
		}

		public virtual void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
		}
	}
}
