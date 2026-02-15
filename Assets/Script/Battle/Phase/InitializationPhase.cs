using UnityEngine; 

namespace Battle.Phase
{
	class InitializationPhase : Battle.Phase.AbstractPhase
	{
		public override void Configure(GameObject playerObject)
		{
			
		}

		public override void OnEnter()
		{

		}

		public override void OnUpdate()
		{
			Manager.SetPhase(Battle.Phase.Mode.Placement);
		}

		public override void OnExit()
		{

		}
	}
}