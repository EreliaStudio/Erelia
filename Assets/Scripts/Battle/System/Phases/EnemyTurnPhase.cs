using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyTurnPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.EnemyTurn;

	public override void Enter()
	{
		if (TurnContext?.ActiveUnit == null)
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		if (!TryComposeAction() &&
			!(ServiceLocator.Instance?.BattleActionCompositionService?.TryComposeEndTurn() ?? false))
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
		}
	}

	private bool TryComposeAction()
	{
		BattleUnit activeUnit = TurnContext.ActiveUnit;
		if (activeUnit == null)
		{
			return false;
		}

		if (TryGetAbilityTargetingEnemy(activeUnit, out Ability ability, out IReadOnlyList<Vector3Int> targetCells))
		{
			BattleActionCompositionService compositionService = ServiceLocator.Instance?.BattleActionCompositionService;
			return compositionService != null &&
				compositionService.TrySelectAbility(ability) &&
				compositionService.TryComposeAbilityTargets(targetCells);
		}

		IReadOnlyList<Vector3Int> reachableCells = BattleActionValidator.GetReachableCells(BattleContext, TurnContext);
		if (reachableCells.Count > 0)
		{
			Vector3Int destination = PickCellTowardNearestEnemy(activeUnit, reachableCells);
			return ServiceLocator.Instance?.BattleActionCompositionService?.TryComposeMovement(destination) ?? false;
		}

		return false;
	}

	private bool TryGetAbilityTargetingEnemy(BattleUnit unit, out Ability ability, out IReadOnlyList<Vector3Int> targetCells)
	{
		ability = null;
		targetCells = null;
		if (unit?.Abilities == null)
		{
			return false;
		}

		for (int index = 0; index < unit.Abilities.Count; index++)
		{
			Ability candidate = unit.Abilities[index];
			if (candidate == null || !BattleActionValidator.CanUseAbility(BattleContext, TurnContext, candidate))
			{
				continue;
			}

			IReadOnlyList<Vector3Int> validCells = BattleActionValidator.GetValidTargetCells(BattleContext, TurnContext, candidate);
			for (int cellIndex = 0; cellIndex < validCells.Count; cellIndex++)
			{
				if (BattleContext.Board.TryGetUnitAt(validCells[cellIndex], out BattleUnit target) &&
					target != null &&
					!target.IsDefeated &&
					target.Side != unit.Side)
				{
					ability = candidate;
					targetCells = new[] { validCells[cellIndex] };
					return true;
				}
			}
		}

		return false;
	}

	private Vector3Int PickCellTowardNearestEnemy(BattleUnit unit, IReadOnlyList<Vector3Int> reachableCells)
	{
		BattleUnit nearestEnemy = FindNearestEnemy(unit);
		if (nearestEnemy == null)
		{
			return reachableCells[0];
		}

		Vector3Int best = reachableCells[0];
		int bestDistance = ManhattanDistance(best, nearestEnemy.BoardPosition);

		for (int index = 1; index < reachableCells.Count; index++)
		{
			int distance = ManhattanDistance(reachableCells[index], nearestEnemy.BoardPosition);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				best = reachableCells[index];
			}
		}

		return best;
	}

	private BattleUnit FindNearestEnemy(BattleUnit unit)
	{
		BattleUnit nearest = null;
		int nearestDistance = int.MaxValue;

		foreach (BattleUnit opponent in BattleContext.GetOpponents(unit))
		{
			if (opponent == null || opponent.IsDefeated || !opponent.HasBoardPosition)
			{
				continue;
			}

			int distance = ManhattanDistance(unit.BoardPosition, opponent.BoardPosition);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = opponent;
			}
		}

		return nearest;
	}

	private static int ManhattanDistance(Vector3Int a, Vector3Int b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.z - b.z);
	}
}
