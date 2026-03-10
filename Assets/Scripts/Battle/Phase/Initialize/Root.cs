using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	/// <summary>
	/// Initialization phase that prepares battle data.
	/// Uses the preselected enemy team, computes acceptable floor coordinates, partitions player/enemy placement areas,
	/// then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		/// <summary>
		/// Whether initialization is still pending.
		/// </summary>
		private bool pendingSetup;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Initialize;

		/// <summary>
		/// Enters the initialize phase and prepares battle data.
		/// </summary>
		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			// Try to initialize battle data and request the next phase.
			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		/// <summary>
		/// Ticks the initialize phase until setup succeeds.
		/// </summary>
		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			// Retry setup while it is still pending.
			if (!pendingSetup)
			{
				return;
			}

			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data for the current encounter.
		/// </summary>
		private bool TrySetupBattleData()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData == null || battleData.Board == null || battleData.EnemyTeam == null)
			{
				return false;
			}

			if (battleData.PhaseInfo == null)
			{
				battleData.PhaseInfo = new Erelia.Battle.Phase.Info();
			}

			battleData.PhaseInfo.Clear();
			battleData.PhaseInfo.SetEnemyTeam(battleData.EnemyTeam);

			if (!TrySetupPlacementAreas(battleData))
			{
				return false;
			}

			return true;
		}

		private bool TrySetupPlacementAreas(Erelia.Battle.Data battleData)
		{
			Erelia.Battle.Phase.Info phaseInfo = battleData.PhaseInfo;
			if (phaseInfo == null ||
				!Erelia.Battle.Phase.Initialize.PlacementListGenerator.TryGenerate(
					battleData.Board,
					Erelia.Battle.Phase.Initialize.PlacementMode.HalfBoard,
					out List<Vector3Int> playerCoordinates,
					out List<Vector3Int> enemyCoordinates))
			{
				return false;
			}

			phaseInfo.AddAcceptableCoordinates(playerCoordinates);
			phaseInfo.AddAcceptableCoordinates(enemyCoordinates);
			phaseInfo.AddPlayerPlacementCoordinates(playerCoordinates);
			phaseInfo.AddEnemyPlacementCoordinates(enemyCoordinates);
			return true;
		}
	}
}
