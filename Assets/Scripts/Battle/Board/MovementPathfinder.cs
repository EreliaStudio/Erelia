using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// Computes uniform-cost movement reachability on the battle board using cardinal adjacency.
	/// </summary>
	public static class MovementPathfinder
	{
		private const float MaximumVerticalTraversalGap = 0.5f;

		private static readonly Vector2Int[] CardinalOffsets =
		{
			Vector2Int.left,
			Vector2Int.right,
			Vector2Int.up,
			Vector2Int.down
		};

		public static Dictionary<Vector3Int, List<Vector3Int>> BuildReachablePaths(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter unit)
		{
			var reachablePaths = new Dictionary<Vector3Int, List<Vector3Int>>();
			if (battleData == null || unit == null || !unit.IsAlive || !unit.IsPlaced)
			{
				return reachablePaths;
			}

			Erelia.Battle.Board.Model board = battleData.Board;
			int maxMovementPoints = Mathf.Max(0, unit.RemainingMovementPoints);
			var acceptableCoordinates = new HashSet<Vector3Int>(battleData.AcceptableCoordinates ?? System.Array.Empty<Vector3Int>());
			Dictionary<Vector2Int, List<Vector3Int>> acceptableCoordinatesByColumn =
				BuildColumnLookup(battleData.AcceptableCoordinates);
			if (board == null || maxMovementPoints <= 0)
			{
				return reachablePaths;
			}

			if (acceptableCoordinates.Count == 0)
			{
				return reachablePaths;
			}

			if (!acceptableCoordinates.Contains(unit.Cell))
			{
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
					Vector2Int horizontalOffset = CardinalOffsets[i];
					Vector2Int nextColumn = new Vector2Int(
						current.x + horizontalOffset.x,
						current.z + horizontalOffset.y);
					if (!acceptableCoordinatesByColumn.TryGetValue(nextColumn, out List<Vector3Int> candidateCoordinates) ||
						candidateCoordinates == null)
					{
						continue;
					}

					for (int candidateIndex = 0; candidateIndex < candidateCoordinates.Count; candidateIndex++)
					{
						Vector3Int next = candidateCoordinates[candidateIndex];
						int nextCost = currentCost + 1;
						if (!CanTraverse(battleData, board, acceptableCoordinates, unit, current, next))
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

			return reachablePaths;
		}

		private static Dictionary<Vector2Int, List<Vector3Int>> BuildColumnLookup(IEnumerable<Vector3Int> coordinates)
		{
			var lookup = new Dictionary<Vector2Int, List<Vector3Int>>();
			if (coordinates == null)
			{
				return lookup;
			}

			foreach (Vector3Int coordinate in coordinates)
			{
				Vector2Int key = new Vector2Int(coordinate.x, coordinate.z);
				if (!lookup.TryGetValue(key, out List<Vector3Int> entries) || entries == null)
				{
					entries = new List<Vector3Int>();
					lookup[key] = entries;
				}

				entries.Add(coordinate);
			}

			return lookup;
		}

		private static bool CanTraverse(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Board.Model board,
			HashSet<Vector3Int> acceptableCoordinates,
			Erelia.Battle.Unit.Presenter unit,
			Vector3Int currentCoordinate,
			Vector3Int coordinate)
		{
			if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
			{
				return false;
			}

			if (!acceptableCoordinates.Contains(coordinate))
			{
				return false;
			}

			if (!Erelia.Battle.Board.UnitPlacementUtility.TryCanTraverseMovementStep(
					board,
					currentCoordinate,
					coordinate,
					MaximumVerticalTraversalGap,
					out Vector3 pointDelta))
			{
				return false;
			}

			if (battleData.TryGetPlacedUnitAtCell(coordinate, out Erelia.Battle.Unit.Presenter occupyingUnit) &&
				!ReferenceEquals(occupyingUnit, unit))
			{
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
