using System;
using UnityEngine;

namespace Erelia.Battle
{
	[Serializable]
	public sealed class BattlePhaseRegistry
	{
		[SerializeField] private InitializePhase initialize = new InitializePhase();
		[SerializeField] private PlacementPhase placement = new PlacementPhase();
		[SerializeField] private PlayerTurnPhase playerTurn = new PlayerTurnPhase();
		[SerializeField] private EnemyTurnPhase enemyTurn = new EnemyTurnPhase();
		[SerializeField] private ResolveActionPhase resolveAction = new ResolveActionPhase();
		[SerializeField] private VictoryPhase victory = new VictoryPhase();
		[SerializeField] private DefeatPhase defeat = new DefeatPhase();
		[SerializeField] private CleanupPhase cleanup = new CleanupPhase();

		public InitializePhase Initialize => initialize;
		public PlacementPhase Placement => placement;
		public PlayerTurnPhase PlayerTurn => playerTurn;
		public EnemyTurnPhase EnemyTurn => enemyTurn;
		public ResolveActionPhase ResolveAction => resolveAction;
		public VictoryPhase Victory => victory;
		public DefeatPhase Defeat => defeat;
		public CleanupPhase Cleanup => cleanup;

		public BattlePhase[] GetAllPhases()
		{
			return new BattlePhase[]
			{
				initialize,
				placement,
				playerTurn,
				enemyTurn,
				resolveAction,
				victory,
				defeat,
				cleanup
			};
		}
	}
}
