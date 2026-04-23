using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlacementPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Placement;

	private IReadOnlyList<Vector3Int> playerPlacementCells = Array.Empty<Vector3Int>();
	private IReadOnlyList<Vector3Int> enemyPlacementCells = Array.Empty<Vector3Int>();

	public override void Enter()
	{
		if (BattleContext?.Board == null)
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		if (!BattlePlacementRules.TryBuildPlacementZones(BattleContext, out BattlePlacementRules.PlacementZones zones))
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		playerPlacementCells = zones.PlayerCells;
		enemyPlacementCells = zones.EnemyCells;

		if (!BattlePlacementRules.TryAutoPlaceUnitsRandomly(
			BattleContext,
			BattlePlacementRules.GetUnitsWaitingForPlacement(BattleContext, BattleSide.Enemy),
			enemyPlacementCells))
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		if (CanCompletePlacement())
		{
			TryCompletePlacement();
		}
	}

	public override void Exit()
	{
	}

	public IReadOnlyList<Vector3Int> GetPlayerPlacementCells()
	{
		return playerPlacementCells;
	}

	public IReadOnlyList<Vector3Int> GetEnemyPlacementCells()
	{
		return enemyPlacementCells;
	}

	public IReadOnlyList<BattleUnit> GetUnitsWaitingForPlacement()
	{
		return BattlePlacementRules.GetUnitsWaitingForPlacement(BattleContext, BattleSide.Player);
	}

	public IReadOnlyList<Vector3Int> GetValidPlacementCells(BattleUnit unit)
	{
		if (!CanPlaceUnit(unit))
		{
			return Array.Empty<Vector3Int>();
		}

		return BattlePlacementRules.GetValidPlacementCells(BattleContext, unit, playerPlacementCells);
	}

	public IReadOnlyList<Vector3Int> GetValidPlacementCells(CreatureUnit creature)
	{
		return !TryGetPlayerBattleUnit(creature, out BattleUnit unit)
			? Array.Empty<Vector3Int>()
			: GetValidPlacementCells(unit);
	}

	public bool CanPlaceUnit(BattleUnit unit)
	{
		return unit != null &&
			unit.Side == BattleSide.Player &&
			!unit.IsDefeated &&
			ContainsUnit(BattleContext?.PlayerUnits, unit);
	}

	public bool CanPlaceUnit(CreatureUnit creature, Vector3Int cell)
	{
		return TryGetPlayerBattleUnit(creature, out BattleUnit unit) && CanPlaceUnit(unit, cell);
	}

	public bool CanPlaceUnit(BattleUnit unit, Vector3Int cell)
	{
		return CanPlaceUnit(unit) &&
			BattlePlacementRules.CanPlaceUnit(BattleContext, unit, cell, playerPlacementCells);
	}

	public bool TryPlaceUnit(BattleUnit unit, Vector3Int cell)
	{
		if (!CanPlaceUnit(unit, cell))
		{
			return false;
		}

		bool placed = BattleContext.TryPlaceUnit(unit, cell);
		return placed;
	}

	public bool TryPlaceUnit(CreatureUnit creature, Vector3Int cell)
	{
		return TryGetPlayerBattleUnit(creature, out BattleUnit unit) && TryPlaceUnit(unit, cell);
	}

	public bool CanCompletePlacement()
	{
		if (BattleContext == null)
		{
			return false;
		}

		for (int index = 0; index < BattleContext.PlayerUnits.Count; index++)
		{
			BattleUnit unit = BattleContext.PlayerUnits[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			if (!unit.HasBoardPosition || !ContainsCell(playerPlacementCells, unit.BoardPosition))
			{
				return false;
			}
		}

		return true;
	}

	public bool TryCompletePlacement()
	{
		if (!CanCompletePlacement())
		{
			return false;
		}

		if (Orchestrator.TryBeginNextTurn(out BattlePhaseType nextPhase))
		{
			Coordinator.TransitionTo(nextPhase);
			return true;
		}

		Coordinator.TransitionTo(BattlePhaseType.End);
		return false;
	}

	public IReadOnlyList<Vector3Int> GetPlacementMaskCells()
	{
		return playerPlacementCells;
	}

	private static bool ContainsCell(IReadOnlyList<Vector3Int> cells, Vector3Int targetCell)
	{
		if (cells == null)
		{
			return false;
		}

		for (int index = 0; index < cells.Count; index++)
		{
			if (cells[index] == targetCell)
			{
				return true;
			}
		}

		return false;
	}

	private static bool ContainsUnit(IReadOnlyList<BattleUnit> units, BattleUnit targetUnit)
	{
		if (units == null || targetUnit == null)
		{
			return false;
		}

		for (int index = 0; index < units.Count; index++)
		{
			if (ReferenceEquals(units[index], targetUnit))
			{
				return true;
			}
		}

		return false;
	}

	private bool TryGetPlayerBattleUnit(CreatureUnit creature, out BattleUnit battleUnit)
	{
		battleUnit = null;
		if (creature == null || BattleContext?.PlayerUnits == null)
		{
			return false;
		}

		for (int index = 0; index < BattleContext.PlayerUnits.Count; index++)
		{
			BattleUnit candidate = BattleContext.PlayerUnits[index];
			if (candidate != null && ReferenceEquals(candidate.SourceUnit, creature))
			{
				battleUnit = candidate;
				return true;
			}
		}

		return false;
	}
}
