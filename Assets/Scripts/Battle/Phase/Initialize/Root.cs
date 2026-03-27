using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private const float DefaultStageSideOffset = 2.5f;
		private const float DefaultStageLaneInset = 0.5f;
		private const float DefaultStageHeight = 1f;

		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Transform playerUnitsRoot;
		[SerializeField] private Transform enemyUnitsRoot;
		[SerializeField] private GameObject battleUnitHealthBarPrefab;
		[SerializeField] private Vector3 stagedUnitSpacing = new Vector3(0f, 0f, 2f);

		private bool pendingSetup;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Initialize;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			pendingSetup = !TrySetupBattle();
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

			pendingSetup = !TrySetupBattle();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		private bool TrySetupBattle()
		{
			Erelia.Battle.BattleState battle = Erelia.Core.GameContext.Instance.Battle;
			if (battle == null || battle.Board == null || battle.EnemyTeam == null)
			{
				return false;
			}

			battle.ClearRuntime();
			if (!TrySetupPlacementAreas(battle))
			{
				return false;
			}

			CreateUnits(battle, Erelia.Core.GameContext.Instance.PlayerParty?.PlayerTeam, Erelia.Battle.Side.Player);
			CreateUnits(battle, battle.EnemyTeam, Erelia.Battle.Side.Enemy);
			battle.FeatProgressTracker.BeginBattle(battle.PlayerUnits);
			return true;
		}

		private bool TrySetupPlacementAreas(Erelia.Battle.BattleState battle)
		{
			if (!Erelia.Battle.Phase.Initialize.AcceptableCoordinateListGenerator.TryGenerate(
					battle.Board,
					out List<Vector3Int> acceptableCoordinates))
			{
				return false;
			}

			battle.AddAcceptableCoordinates(acceptableCoordinates);
			return true;
		}

		private void CreateUnits(
			Erelia.Battle.BattleState battle,
			Erelia.Core.Creature.Team team,
			Erelia.Battle.Side side)
		{
			Erelia.Core.Creature.Instance.CreatureInstance[] slots = team?.Slots;
			if (battle == null || slots == null)
			{
				return;
			}

			Transform parent = ResolveUnitsRoot(side);
			GameObject healthBarPrefab = ResolveHealthBarPrefab();
			Vector3 baseWorldPosition = ResolveStageBaseWorldPosition(battle.Board, side, parent);
			for (int i = 0; i < slots.Length; i++)
			{
				Erelia.Core.Creature.Instance.CreatureInstance creature = slots[i];
				if (creature == null || creature.IsEmpty)
				{
					continue;
				}

				var unit = new Erelia.Battle.Unit.BattleUnit(creature, side);
				Erelia.Battle.Unit.Presenter unitPresenter = CreateUnitPresenter(unit, parent, healthBarPrefab);
				if (unitPresenter == null)
				{
					continue;
				}

				unitPresenter.Stage(baseWorldPosition + ResolveStageOffset(side, i));
				battle.AddUnit(unitPresenter);
			}
		}

		private Erelia.Battle.Unit.Presenter CreateUnitPresenter(
			Erelia.Battle.Unit.BattleUnit unit,
			Transform parent,
			GameObject healthBarPrefab)
		{
			if (unit == null)
			{
				return null;
			}

			GameObject unitObject = InstantiateUnitObject(unit, parent);
			Erelia.Battle.Unit.Presenter presenter = ResolveUnitPresenter(unitObject);
			if (presenter == null)
			{
				return null;
			}

			presenter.SetHealthBarPrefab(healthBarPrefab);
			presenter.SetUnit(unit);
			return presenter;
		}

		private GameObject InstantiateUnitObject(Erelia.Battle.Unit.BattleUnit unit, Transform parent)
		{
			if (!TryResolveUnitPrefab(unit, out GameObject unitPrefab) || unitPrefab == null)
			{
				GameObject fallbackObject = new GameObject("BattleUnit");
				if (parent != null)
				{
					fallbackObject.transform.SetParent(parent, false);
				}

				return fallbackObject;
			}

			GameObject unitObject = parent != null
				? Object.Instantiate(unitPrefab, parent, false)
				: Object.Instantiate(unitPrefab);
			unitObject.transform.localPosition = Vector3.zero;
			unitObject.transform.localRotation = Quaternion.identity;
			return unitObject;
		}

		private static Erelia.Battle.Unit.Presenter ResolveUnitPresenter(GameObject unitObject)
		{
			if (unitObject == null)
			{
				return null;
			}

			Erelia.Battle.Unit.Presenter presenter = unitObject.GetComponent<Erelia.Battle.Unit.Presenter>();
			if (presenter != null)
			{
				return presenter;
			}

			Debug.LogWarning("[Erelia.Battle.Phase.Initialize.Root] Unit prefab is missing Battle.Unit.Presenter. Adding a fallback presenter component.");
			return unitObject.AddComponent<Erelia.Battle.Unit.Presenter>();
		}

		private static bool TryResolveUnitPrefab(Erelia.Battle.Unit.BattleUnit unit, out GameObject unitPrefab)
		{
			unitPrefab = unit != null ? unit.UnitPrefab : null;
			if (unitPrefab != null)
			{
				return true;
			}

			if (unit == null || !unit.TryGetSpecies(out Erelia.Core.Creature.Species species))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Initialize.Root] Failed to resolve unit species.");
				return false;
			}

			Debug.LogWarning($"[Erelia.Battle.Phase.Initialize.Root] Creature '{species.DisplayName}' has no unit prefab on its current form or species.");
			return false;
		}

		private GameObject ResolveHealthBarPrefab()
		{
			return battleUnitHealthBarPrefab;
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

		private Vector3 ResolveStageBaseWorldPosition(
			Erelia.Battle.Board.BattleBoardState board,
			Erelia.Battle.Side side,
			Transform fallbackParent)
		{
			if (TryResolveStageBaseLocalPosition(board, side, out Vector3 localPosition))
			{
				return boardPresenter != null
					? boardPresenter.transform.TransformPoint(localPosition)
					: localPosition;
			}

			return fallbackParent != null ? fallbackParent.position : Vector3.zero;
		}

		private static bool TryResolveStageBaseLocalPosition(
			Erelia.Battle.Board.BattleBoardState board,
			Erelia.Battle.Side side,
			out Vector3 localPosition)
		{
			localPosition = default;
			if (board == null)
			{
				return false;
			}

			float lanePosition = side == Erelia.Battle.Side.Player
				? DefaultStageLaneInset
				: Mathf.Max(DefaultStageLaneInset, board.SizeZ - DefaultStageLaneInset);
			float xPosition = side == Erelia.Battle.Side.Player
				? -DefaultStageSideOffset
				: board.SizeX + DefaultStageSideOffset;

			localPosition = new Vector3(xPosition, DefaultStageHeight, lanePosition);
			return true;
		}

		private Vector3 ResolveStageOffset(Erelia.Battle.Side side, int index)
		{
			int safeIndex = Mathf.Max(0, index);
			Vector3 sideSpacing = stagedUnitSpacing;
			if (side == Erelia.Battle.Side.Enemy)
			{
				sideSpacing.z = -sideSpacing.z;
			}

			return sideSpacing * safeIndex;
		}
	}
}


