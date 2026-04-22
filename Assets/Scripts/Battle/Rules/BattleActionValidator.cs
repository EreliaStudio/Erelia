using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleActionValidator
{
	public static bool CanMoveTo(BattleContext battleContext, TurnContext turnContext, Vector3Int destination)
	{
		return TryGetMovementCost(battleContext, turnContext, destination, out _);
	}

	public static bool TryGetMovementCost(BattleContext battleContext, TurnContext turnContext, Vector3Int destination, out int movementCost)
	{
		movementCost = -1;
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (battleContext?.Board == null ||
			activeUnit == null ||
			activeUnit.IsDefeated ||
			activeUnit.BattleAttributes.MovementPoints.Current <= 0 ||
			!activeUnit.HasBoardPosition)
		{
			return false;
		}

		Dictionary<Vector3Int, int> reachableCosts = BuildReachableCosts(battleContext, activeUnit);
		if (!reachableCosts.TryGetValue(destination, out movementCost))
		{
			return false;
		}

		return movementCost > 0;
	}

	public static IReadOnlyList<Vector3Int> GetReachableCells(BattleContext battleContext, TurnContext turnContext)
	{
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (battleContext == null || activeUnit == null || activeUnit.IsDefeated)
		{
			return Array.Empty<Vector3Int>();
		}

		List<Vector3Int> reachableCells = new List<Vector3Int>();
		foreach (KeyValuePair<Vector3Int, int> pair in BuildReachableCosts(battleContext, activeUnit))
		{
			if (pair.Value <= 0)
			{
				continue;
			}

			reachableCells.Add(pair.Key);
		}

		return reachableCells;
	}

	public static bool CanUseAbility(BattleContext battleContext, TurnContext turnContext, Ability ability)
	{
		return GetValidTargetCells(battleContext, turnContext, ability).Count > 0;
	}

	public static AbilityCastLegality GetCastLegality(BattleContext battleContext, TurnContext turnContext, Ability ability, Vector3Int targetCell)
	{
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (battleContext == null || ability == null)
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.InvalidContext, targetCell);
		}

		if (activeUnit == null || activeUnit.IsDefeated)
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.NoActiveUnit, targetCell);
		}

		if (!activeUnit.HasBoardPosition)
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.SourceNotPlaced, targetCell);
		}

		int requiredActionPoints = Math.Max(0, ability.Cost?.Ability ?? 0);
		int requiredMovementPoints = Math.Max(0, ability.Cost?.Movement ?? 0);
		if (activeUnit.BattleAttributes.ActionPoints.Current < requiredActionPoints ||
			activeUnit.BattleAttributes.MovementPoints.Current < requiredMovementPoints)
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.InsufficientResources, targetCell);
		}

		if (!battleContext.Board.IsInside(targetCell))
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.OutOfBoard, targetCell);
		}

		if (!IsCellInAbilityRange(activeUnit, targetCell, ability))
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.OutOfRange, targetCell);
		}

		if ((ability.Range?.RequireLineOfSight ?? false) &&
			!BattleLineOfSightRules.HasLineOfSight(battleContext, activeUnit.BoardPosition, targetCell))
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.BlockedByLineOfSight, targetCell);
		}

		if (!MatchesTargetProfile(battleContext, activeUnit, ability, targetCell))
		{
			return AbilityCastLegality.Invalid(AbilityCastLegality.Failure.InvalidTargetProfile, targetCell);
		}

		return AbilityCastLegality.Valid(targetCell);
	}

	public static bool CanCastAtCell(BattleContext battleContext, TurnContext turnContext, Ability ability, Vector3Int targetCell)
	{
		return GetCastLegality(battleContext, turnContext, ability, targetCell).IsValid;
	}

	public static bool CanTarget(BattleContext battleContext, TurnContext turnContext, Ability ability, BattleObject target)
	{
		if (ability == null || target == null || battleContext?.Board == null)
		{
			return false;
		}

		if (!battleContext.Board.TryGetPosition(target, out Vector3Int targetCell))
		{
			return false;
		}

		IReadOnlyList<Vector3Int> validCells = GetValidTargetCells(battleContext, turnContext, ability);
		for (int index = 0; index < validCells.Count; index++)
		{
			if (validCells[index] == targetCell)
			{
				return true;
			}
		}

		return false;
	}

	public static bool CanTargetCell(BattleContext battleContext, TurnContext turnContext, Ability ability, Vector3Int cell)
	{
		return CanCastAtCell(battleContext, turnContext, ability, cell);
	}

	public static IReadOnlyList<BattleObject> GetValidTargets(BattleContext battleContext, TurnContext turnContext, Ability ability)
	{
		List<BattleObject> validTargets = new List<BattleObject>();
		IReadOnlyList<Vector3Int> validCells = GetValidTargetCells(battleContext, turnContext, ability);
		for (int index = 0; index < validCells.Count; index++)
		{
			IReadOnlyList<BattleObject> objectsAtCell = battleContext.GetObjectsAt(validCells[index]);
			for (int objectIndex = 0; objectIndex < objectsAtCell.Count; objectIndex++)
			{
				BattleObject candidate = objectsAtCell[objectIndex];
				if (candidate != null && !validTargets.Contains(candidate))
				{
					validTargets.Add(candidate);
				}
			}
		}

		return validTargets;
	}

	public static IReadOnlyList<Vector3Int> GetValidTargetCells(BattleContext battleContext, TurnContext turnContext, Ability ability)
	{
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (battleContext?.Board?.Navigation?.Nodes == null ||
			ability == null ||
			activeUnit == null ||
			!activeUnit.HasBoardPosition)
		{
			return Array.Empty<Vector3Int>();
		}

		List<Vector3Int> validCells = new List<Vector3Int>();
		IReadOnlyList<VoxelTraversalGraph.Node> nodes = battleContext.Board.Navigation.Nodes;
		for (int index = 0; index < nodes.Count; index++)
		{
			Vector3Int cell = nodes[index].Position;
			if (!GetCastLegality(battleContext, turnContext, ability, cell).IsValid)
			{
				continue;
			}

			validCells.Add(cell);
		}

		return validCells;
	}

	public static bool CanEndTurn(BattleContext battleContext, TurnContext turnContext)
	{
		return battleContext != null && turnContext?.ActiveUnit != null && !turnContext.HasPendingAction;
	}

	public static bool TryGetPathTo(BattleContext battleContext, TurnContext turnContext, Vector3Int destination, out IReadOnlyList<Vector3Int> path)
	{
		path = Array.Empty<Vector3Int>();
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (!TryGetMovementCost(battleContext, turnContext, destination, out _) || activeUnit == null)
		{
			return false;
		}

		BuildReachableData(battleContext, activeUnit, out _, out Dictionary<Vector3Int, Vector3Int?> predecessors);
		if (!predecessors.ContainsKey(destination))
		{
			return false;
		}

		path = ReconstructPath(activeUnit.BoardPosition, destination, predecessors);
		return true;
	}

	private static IReadOnlyList<Vector3Int> ReconstructPath(Vector3Int start, Vector3Int destination, Dictionary<Vector3Int, Vector3Int?> predecessors)
	{
		List<Vector3Int> path = new List<Vector3Int>();
		Vector3Int current = destination;
		while (current != start)
		{
			path.Add(current);
			if (!predecessors.TryGetValue(current, out Vector3Int? prev) || prev == null)
			{
				break;
			}

			current = prev.Value;
		}

		path.Reverse();
		return path;
	}

	private static Dictionary<Vector3Int, int> BuildReachableCosts(BattleContext battleContext, BattleUnit activeUnit)
	{
		Dictionary<Vector3Int, int> costs = new Dictionary<Vector3Int, int>();
		if (battleContext?.Board?.Navigation?.Graph == null || activeUnit == null || !activeUnit.HasBoardPosition)
		{
			return costs;
		}

		int maxCost = Math.Max(0, activeUnit.BattleAttributes.MovementPoints.Current);
		Queue<Vector3Int> frontier = new Queue<Vector3Int>();
		costs[activeUnit.BoardPosition] = 0;
		frontier.Enqueue(activeUnit.BoardPosition);

		while (frontier.Count > 0)
		{
			Vector3Int current = frontier.Dequeue();
			int currentCost = costs[current];
			if (currentCost >= maxCost || !battleContext.Board.Navigation.TryGetNode(current, out VoxelTraversalGraph.Node node))
			{
				continue;
			}

			TryVisitNeighbour(node.PositiveX, currentCost + 1);
			TryVisitNeighbour(node.NegativeX, currentCost + 1);
			TryVisitNeighbour(node.PositiveZ, currentCost + 1);
			TryVisitNeighbour(node.NegativeZ, currentCost + 1);
		}

		return costs;

		void TryVisitNeighbour(VoxelTraversalGraph.Node neighbour, int nextCost)
		{
			if (neighbour == null || nextCost > maxCost || costs.ContainsKey(neighbour.Position))
			{
				return;
			}

			if (battleContext.Board.HasUnitAt(neighbour.Position) &&
				(!battleContext.Board.TryGetUnitAt(neighbour.Position, out BattleUnit occupyingUnit) || occupyingUnit != activeUnit))
			{
				return;
			}

			costs[neighbour.Position] = nextCost;
			frontier.Enqueue(neighbour.Position);
		}
	}

	private static bool IsValidTargetUnit(BattleUnit source, Ability ability, BattleUnit candidate)
	{
		if (source == null || candidate == null || candidate.IsDefeated)
		{
			return false;
		}

		return ability.TargetProfile switch
		{
			TargetProfile.Everything => true,
			TargetProfile.Ally => candidate.Side == source.Side,
			TargetProfile.Enemy => candidate.Side != source.Side,
			_ => false
		};
	}

	private static bool MatchesTargetProfile(BattleContext battleContext, BattleUnit source, Ability ability, Vector3Int cell)
	{
		switch (ability.TargetProfile)
		{
			case TargetProfile.Empty:
				return !battleContext.Board.HasUnitAt(cell);

			case TargetProfile.Everything:
				return true;

			case TargetProfile.Ally:
			case TargetProfile.Enemy:
				if (!battleContext.Board.TryGetUnitAt(cell, out BattleUnit unit) || unit == null)
				{
					return false;
				}

				return IsValidTargetUnit(source, ability, unit);

			default:
				return false;
		}
	}

	private static void BuildReachableData(BattleContext battleContext, BattleUnit activeUnit, out Dictionary<Vector3Int, int> costs, out Dictionary<Vector3Int, Vector3Int?> predecessors)
	{
		costs = new Dictionary<Vector3Int, int>();
		predecessors = new Dictionary<Vector3Int, Vector3Int?>();

		if (battleContext?.Board?.Navigation?.Graph == null || activeUnit == null || !activeUnit.HasBoardPosition)
		{
			return;
		}

		int maxCost = Math.Max(0, activeUnit.BattleAttributes.MovementPoints.Current);
		Queue<Vector3Int> frontier = new Queue<Vector3Int>();
		costs[activeUnit.BoardPosition] = 0;
		predecessors[activeUnit.BoardPosition] = null;
		frontier.Enqueue(activeUnit.BoardPosition);

		while (frontier.Count > 0)
		{
			Vector3Int current = frontier.Dequeue();
			int currentCost = costs[current];
			if (currentCost >= maxCost || !battleContext.Board.Navigation.TryGetNode(current, out VoxelTraversalGraph.Node node))
			{
				continue;
			}

			TryVisitNeighbour(node.PositiveX, current, currentCost + 1);
			TryVisitNeighbour(node.NegativeX, current, currentCost + 1);
			TryVisitNeighbour(node.PositiveZ, current, currentCost + 1);
			TryVisitNeighbour(node.NegativeZ, current, currentCost + 1);
		}

		void TryVisitNeighbour(VoxelTraversalGraph.Node neighbour, Vector3Int from, int nextCost)
		{
			if (neighbour == null || nextCost > maxCost || costs.ContainsKey(neighbour.Position))
			{
				return;
			}

			if (battleContext.Board.HasUnitAt(neighbour.Position) &&
				(!battleContext.Board.TryGetUnitAt(neighbour.Position, out BattleUnit occupyingUnit) || occupyingUnit != activeUnit))
			{
				return;
			}

			costs[neighbour.Position] = nextCost;
			predecessors[neighbour.Position] = from;
			frontier.Enqueue(neighbour.Position);
		}
	}

	private static bool IsCellInAbilityRange(BattleUnit source, Vector3Int cell, Ability ability)
	{
		if (source == null || ability == null || !source.HasBoardPosition)
		{
			return false;
		}

		Vector3Int delta = cell - source.BoardPosition;
		int distanceX = Math.Abs(delta.x);
		int distanceZ = Math.Abs(delta.z);
		int rangeValue = Math.Max(0, ability.Range?.Value ?? 0) + Math.Max(0, source.BattleAttributes.BonusRange.Value);

		return (ability.Range?.Type ?? Ability.RangeDefinition.Shape.Circle) switch
		{
			Ability.RangeDefinition.Shape.Circle => distanceX + distanceZ <= rangeValue,
			Ability.RangeDefinition.Shape.Line => (distanceX == 0 || distanceZ == 0) && distanceX + distanceZ <= rangeValue,
			Ability.RangeDefinition.Shape.Diagonal => distanceX == distanceZ && distanceX <= rangeValue,
			_ => false
		};
	}
}
