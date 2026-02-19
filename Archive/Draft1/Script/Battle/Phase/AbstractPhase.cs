using UnityEngine;

namespace Battle.Phase
{
	public abstract class AbstractPhase
	{
		protected Battle.Phase.Manager manager;
		public Battle.Phase.Manager Manager => manager;

		public void Initialize(Battle.Phase.Manager manager, GameObject playerObject)
		{
			this.manager = manager;
			Configure(playerObject);
		}
		abstract public void Configure(GameObject playerObject);

		abstract public void OnEnter();
		abstract public void OnUpdate();
		abstract public void OnExit();
	}
}