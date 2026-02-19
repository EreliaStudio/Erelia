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
			Battle.Context.Service contextService = Utils.ServiceLocator.Instance.BattleContextService;
			if (contextService != null)
			{
				contextService.InitializeFromPlayerTeam(Utils.ServiceLocator.Instance.PlayerTeam);
				contextService.InitializeEnemyPlacementAreas();
			}
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
