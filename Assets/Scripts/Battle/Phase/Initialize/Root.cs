using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	/// <summary>
	/// Initialization phase that prepares battle data.
	/// Uses the preselected enemy team, computes acceptable floor coordinates, instantiates battle units,
	/// and transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Transform playerUnitsRoot;
		[SerializeField] private Transform enemyUnitsRoot;
		[SerializeField] private Vector3 stagedUnitSpacing = new Vector3(0f, 0f, 2f);

		private bool pendingSetup;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Initialize;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
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

		private bool TrySetupBattleData()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData == null || battleData.Board == null || battleData.EnemyTeam == null)
			{
				return false;
			}

			battleData.ClearRuntime();
			if (!TrySetupPlacementAreas(battleData))
			{
				return false;
			}

			CreateUnits(battleData, Erelia.Core.Context.Instance.SystemData?.PlayerTeam, Erelia.Battle.Side.Player);
			CreateUnits(battleData, battleData.EnemyTeam, Erelia.Battle.Side.Enemy);
			return true;
		}

		private bool TrySetupPlacementAreas(Erelia.Battle.Data battleData)
		{
			if (!Erelia.Battle.Phase.Initialize.AcceptableCoordinateListGenerator.TryGenerate(
					battleData.Board,
					out List<Vector3Int> acceptableCoordinates))
			{
				return false;
			}

			battleData.AddAcceptableCoordinates(acceptableCoordinates);
			return true;
		}

		private void CreateUnits(
			Erelia.Battle.Data battleData,
			Erelia.Core.Creature.Team team,
			Erelia.Battle.Side side)
		{
			Erelia.Core.Creature.Instance.Model[] slots = team?.Slots;
			if (battleData == null || slots == null)
			{
				return;
			}

			Transform parent = ResolveUnitsRoot(side);
			Vector3 baseWorldPosition = parent != null ? parent.position : Vector3.zero;
			for (int i = 0; i < slots.Length; i++)
			{
				Erelia.Core.Creature.Instance.Model creature = slots[i];
				if (creature == null || creature.IsEmpty)
				{
					continue;
				}

				var unitModel = new Erelia.Battle.Unit.Model(creature, side);
				var unitPresenter = new Erelia.Battle.Unit.Presenter(unitModel, parent);
				unitPresenter.Stage(baseWorldPosition + ResolveStageOffset(i));
				battleData.AddUnit(unitPresenter);
			}
		}

		private Transform ResolveUnitsRoot(Erelia.Battle.Side side)
		{
			if (side == Erelia.Battle.Side.Player)
			{
				if (playerUnitsRoot == null)
				{
					Debug.LogWarning("[Erelia.Battle.Phase.Initialize.Root] Player units root is not assigned.");
				}

				return playerUnitsRoot;
			}

			if (enemyUnitsRoot == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Initialize.Root] Enemy units root is not assigned.");
			}

			return enemyUnitsRoot;
		}

		private Vector3 ResolveStageOffset(int index)
		{
			int safeIndex = Mathf.Max(0, index);
			return stagedUnitSpacing * safeIndex;
		}
	}
}
