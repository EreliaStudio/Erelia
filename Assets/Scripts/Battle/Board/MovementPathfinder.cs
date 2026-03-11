using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// Computes uniform-cost movement reachability on the battle board using cardinal adjacency.
	/// </summary>
	public static class MovementPathfinder
	{
		private const float MaximumTraversalGap = 0.5f;

		private static readonly Vector3Int[] CardinalOffsets =
		{
			Vector3Int.left,
			Vector3Int.right,
			Vector3Int.forward,
			Vector3Int.back
		};

		public static Dictionary<Vector3Int, List<Vector3Int>> BuildReachablePaths(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter unit,
			bool enableDebugLogs = false)
		{
			var reachablePaths = new Dictionary<Vector3Int, List<Vector3Int>>();
			if (battleData == null || unit == null || !unit.IsAlive || !unit.IsPlaced)
			{
				if (enableDebugLogs)
				{
					Debug.LogWarning("[Erelia.Battle.Board.MovementPathfinder] Cannot build movement paths because the battle data or unit is invalid.");
				}

				return reachablePaths;
			}

			Erelia.Battle.Board.Model board = battleData.Board;
			int maxMovementPoints = Mathf.Max(0, unit.RemainingMovementPoints);
			var acceptableCoordinates = new HashSet<Vector3Int>(battleData.AcceptableCoordinates ?? System.Array.Empty<Vector3Int>());
			if (board == null || maxMovementPoints <= 0)
			{
				if (enableDebugLogs)
				{
					Debug.LogWarning(
						$"[Erelia.Battle.Board.MovementPathfinder] Cannot build movement paths for '{unit.Creature?.DisplayName ?? unit.name}'. " +
						$"Board: {(board != null ? "ok" : "missing")}, remainingMovementPoints: {maxMovementPoints}.");
				}

				return reachablePaths;
			}

			if (acceptableCoordinates.Count == 0)
			{
				if (enableDebugLogs)
				{
					Debug.LogWarning("[Erelia.Battle.Board.MovementPathfinder] No acceptable coordinates are available in battle data.");
				}

				return reachablePaths;
			}

			if (enableDebugLogs)
			{
				Debug.Log(
					$"[Erelia.Battle.Board.MovementPathfinder] Start '{unit.Creature?.DisplayName ?? unit.name}' at {unit.Cell} " +
					$"with {maxMovementPoints} remaining movement points out of {unit.MovementPoints}. " +
					$"Acceptable coordinates: {acceptableCoordinates.Count}.");
			}

			if (!acceptableCoordinates.Contains(unit.Cell))
			{
				if (enableDebugLogs)
				{
					Debug.LogWarning(
						$"[Erelia.Battle.Board.MovementPathfinder] Start cell {unit.Cell} is not in the acceptable coordinate list.");
				}

				return reachablePaths;
			}

			var frontier = new Queue<Vector3Int>();
			var costs = new Dictionary<Vector3Int, int> { [unit.Cell] = 0 };
			var previous = new Dictionary<Vector3Int, Vector3Int>();

			frontier.Enqueue(unit.Cell);
			while (frontier.Count > 0)
			{
				Vector3Int current = frontier.Dequeue();
				int currentCost = costs[current];
				if (currentCost >= maxMovementPoints)
				{
					continue;
				}

				for (int i = 0; i < CardinalOffsets.Length; i++)
				{
					Vector3Int next = current + CardinalOffsets[i];
					int nextCost = currentCost + 1;
					if (!CanTraverse(battleData, board, acceptableCoordinates, unit, current, next, enableDebugLogs))
					{
						continue;
					}

					if (costs.TryGetValue(next, out int existingCost) && existingCost <= nextCost)
					{
						continue;
					}

					costs[next] = nextCost;
					previous[next] = current;
					frontier.Enqueue(next);

					if (enableDebugLogs)
					{
						Debug.Log(
							$"[Erelia.Battle.Board.MovementPathfinder] Added reachable cell {next} at cost {nextCost} from {current}.");
					}
				}
			}

			foreach (KeyValuePair<Vector3Int, int> entry in costs)
			{
				if (entry.Value <= 0)
				{
					continue;
				}

				reachablePaths[entry.Key] = ReconstructPath(unit.Cell, entry.Key, previous);
			}

			if (enableDebugLogs)
			{
				Debug.Log(
					$"[Erelia.Battle.Board.MovementPathfinder] Computed {reachablePaths.Count} reachable cells for " +
					$"'{unit.Creature?.DisplayName ?? unit.name}'.");
			}

			return reachablePaths;
		}

		private static bool CanTraverse(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Board.Model board,
			HashSet<Vector3Int> acceptableCoordinates,
			Erelia.Battle.Unit.Presenter unit,
			Vector3Int currentCoordinate,
			Vector3Int coordinate,
			bool enableDebugLogs)
		{
			if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
			{
				if (enableDebugLogs)
				{
					Debug.Log($"[Erelia.Battle.Board.MovementPathfinder] Skipping {coordinate}: outside board bounds.");
				}

				return false;
			}

			if (!acceptableCoordinates.Contains(coordinate))
			{
				if (enableDebugLogs)
				{
					Debug.Log($"[Erelia.Battle.Board.MovementPathfinder] Skipping {coordinate}: not marked as an acceptable standable cell.");
				}

				return false;
			}

			if (!Erelia.Battle.Board.UnitPlacementUtility.TryCanTraverseMovementStep(
					board,
					currentCoordinate,
					coordinate,
					MaximumTraversalGap,
					out float traversalGap))
			{
				if (enableDebugLogs)
				{
					Debug.Log(
						$"[Erelia.Battle.Board.MovementPathfinder] Skipping {coordinate}: traversal gap from {currentCoordinate} " +
						$"is {traversalGap:0.###}, above the {MaximumTraversalGap:0.###} threshold.");
				}

				return false;
			}

			if (enableDebugLogs)
			{
				Debug.Log(
					$"[Erelia.Battle.Board.MovementPathfinder] Traversal from {currentCoordinate} to {coordinate} accepted " +
					$"with a cardinal-point gap of {traversalGap:0.###}.");
			}

			if (battleData.TryGetPlacedUnitAtCell(coordinate, out Erelia.Battle.Unit.Presenter occupyingUnit) &&
				!ReferenceEquals(occupyingUnit, unit))
			{
				if (enableDebugLogs)
				{
					Debug.Log(
						$"[Erelia.Battle.Board.MovementPathfinder] Skipping {coordinate}: occupied by " +
						$"'{occupyingUnit.Creature?.DisplayName ?? occupyingUnit.name}'.");
				}

				return false;
			}

			return true;
		}

		private static List<Vector3Int> ReconstructPath(
			Vector3Int start,
			Vector3Int destination,
			Dictionary<Vector3Int, Vector3Int> previous)
		{
			var path = new List<Vector3Int>();
			Vector3Int current = destination;
			while (current != start && previous.TryGetValue(current, out Vector3Int next))
			{
				path.Add(current);
				current = next;
			}

			path.Reverse();
			return path;
		}
	}
}
