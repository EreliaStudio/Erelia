using System.Collections.Generic;
using UnityEngine;

public static class WorldPathfinder
{
	private const int DefaultMaxIterations = 4096;

	private readonly struct TraversalCandidate
	{
		public readonly Vector3Int Position;
		public readonly float VerticalGap;
		public readonly int VerticalCellGap;

		public TraversalCandidate(Vector3Int p_position, float p_verticalGap, int p_verticalCellGap)
		{
			Position = p_position;
			VerticalGap = p_verticalGap;
			VerticalCellGap = p_verticalCellGap;
		}
	}

	public static bool TryFindPath(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		WorldTraversalGraphCache p_graphCache,
		Vector3Int p_startWorldPosition,
		Vector3Int p_targetWorldPosition,
		out List<Vector3Int> p_path,
		int p_maxIterations = DefaultMaxIterations)
	{
		p_path = new List<Vector3Int>();

		if (p_worldData == null || p_voxelRegistry == null || p_graphCache == null)
		{
			return false;
		}

		if (!p_graphCache.HasNode(p_worldData, p_voxelRegistry, p_startWorldPosition) ||
			!p_graphCache.HasNode(p_worldData, p_voxelRegistry, p_targetWorldPosition))
		{
			return false;
		}

		var openSet = new List<Vector3Int> { p_startWorldPosition };
		var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
		var gScore = new Dictionary<Vector3Int, float> { [p_startWorldPosition] = 0f };
		var fScore = new Dictionary<Vector3Int, float> { [p_startWorldPosition] = Heuristic(p_startWorldPosition, p_targetWorldPosition) };
		var closedSet = new HashSet<Vector3Int>();
		int iterations = 0;

		while (openSet.Count > 0 && iterations < p_maxIterations)
		{
			iterations++;

			int bestIndex = 0;
			Vector3Int current = openSet[0];
			float bestScore = GetScore(fScore, current);

			for (int i = 1; i < openSet.Count; i++)
			{
				Vector3Int candidate = openSet[i];
				float candidateScore = GetScore(fScore, candidate);
				if (candidateScore < bestScore)
				{
					bestIndex = i;
					current = candidate;
					bestScore = candidateScore;
				}
			}

			if (current == p_targetWorldPosition)
			{
				BuildPath(cameFrom, current, p_path);
				return true;
			}

			openSet.RemoveAt(bestIndex);
			closedSet.Add(current);

			using List<Vector3Int>.Enumerator neighbours = GetNeighbours(p_worldData, p_voxelRegistry, p_graphCache, current).GetEnumerator();
			while (neighbours.MoveNext())
			{
				Vector3Int neighbour = neighbours.Current;
				if (closedSet.Contains(neighbour))
				{
					continue;
				}

				float tentativeGScore = GetScore(gScore, current) + 1f;
				if (tentativeGScore >= GetScore(gScore, neighbour))
				{
					continue;
				}

				cameFrom[neighbour] = current;
				gScore[neighbour] = tentativeGScore;
				fScore[neighbour] = tentativeGScore + Heuristic(neighbour, p_targetWorldPosition);

				if (!openSet.Contains(neighbour))
				{
					openSet.Add(neighbour);
				}
			}
		}

		return false;
	}

	public static bool TryResolveSelectableTarget(WorldData p_worldData, VoxelRegistry p_voxelRegistry, WorldTraversalGraphCache p_graphCache, Vector3Int p_worldPosition, out Vector3Int p_targetWorldPosition)
	{
		p_targetWorldPosition = default;

		if (p_worldData == null || p_voxelRegistry == null || p_graphCache == null)
		{
			return false;
		}

		int clampedY = Mathf.Clamp(p_worldPosition.y, 0, ChunkData.FixedSizeY - 1);
		for (int y = clampedY; y >= 0; y--)
		{
			Vector3Int candidate = new Vector3Int(p_worldPosition.x, y, p_worldPosition.z);
			if (!p_graphCache.HasNode(p_worldData, p_voxelRegistry, candidate))
			{
				continue;
			}

			p_targetWorldPosition = candidate;
			return true;
		}

		return false;
	}

	public static bool TryResolveStandingCell(WorldData p_worldData, VoxelRegistry p_voxelRegistry, WorldTraversalGraphCache p_graphCache, Vector3 p_worldPosition, out Vector3Int p_standingCell)
	{
		p_standingCell = default;

		if (p_worldData == null || p_voxelRegistry == null || p_graphCache == null)
		{
			return false;
		}

		Vector3Int baseCell = Vector3Int.FloorToInt(p_worldPosition);
		int preferredY = Mathf.Clamp(baseCell.y, 0, ChunkData.FixedSizeY - 1);

		for (int offset = 0; offset < ChunkData.FixedSizeY; offset++)
		{
			int downwardY = preferredY - offset;
			if (downwardY >= 0)
			{
				Vector3Int candidate = new Vector3Int(baseCell.x, downwardY, baseCell.z);
				if (p_graphCache.HasNode(p_worldData, p_voxelRegistry, candidate))
				{
					p_standingCell = candidate;
					return true;
				}
			}

			int upwardY = preferredY + offset;
			if (offset > 0 && upwardY < ChunkData.FixedSizeY)
			{
				Vector3Int candidate = new Vector3Int(baseCell.x, upwardY, baseCell.z);
				if (p_graphCache.HasNode(p_worldData, p_voxelRegistry, candidate))
				{
					p_standingCell = candidate;
					return true;
				}
			}
		}

		return false;
	}

	public static bool TryGetStandingWorldPoint(WorldData p_worldData, VoxelRegistry p_voxelRegistry, Vector3Int p_worldPosition, out Vector3 p_worldPoint)
	{
		return VoxelTraversalUtility.TryGetStandingWorldPoint(p_worldData, p_worldPosition, p_voxelRegistry, out p_worldPoint);
	}

	private static List<Vector3Int> GetNeighbours(WorldData p_worldData, VoxelRegistry p_voxelRegistry, WorldTraversalGraphCache p_graphCache, Vector3Int p_position)
	{
		var neighbours = new List<Vector3Int>(4);

		TryAddNeighbour(p_worldData, p_voxelRegistry, p_graphCache, p_position, VoxelTraversalGraph.Node.Direction.PositiveX, neighbours);
		TryAddNeighbour(p_worldData, p_voxelRegistry, p_graphCache, p_position, VoxelTraversalGraph.Node.Direction.NegativeX, neighbours);
		TryAddNeighbour(p_worldData, p_voxelRegistry, p_graphCache, p_position, VoxelTraversalGraph.Node.Direction.PositiveZ, neighbours);
		TryAddNeighbour(p_worldData, p_voxelRegistry, p_graphCache, p_position, VoxelTraversalGraph.Node.Direction.NegativeZ, neighbours);

		return neighbours;
	}

	private static void TryAddNeighbour(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		WorldTraversalGraphCache p_graphCache,
		Vector3Int p_position,
		VoxelTraversalGraph.Node.Direction p_direction,
		List<Vector3Int> p_neighbours)
	{
		if (TryFindAdjacentNeighbour(p_worldData, p_voxelRegistry, p_graphCache, p_position, p_direction, out Vector3Int neighbourWorldPosition))
		{
			p_neighbours.Add(neighbourWorldPosition);
		}
	}

	private static bool TryFindAdjacentNeighbour(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		WorldTraversalGraphCache p_graphCache,
		Vector3Int p_position,
		VoxelTraversalGraph.Node.Direction p_direction,
		out Vector3Int p_neighbourWorldPosition)
	{
		p_neighbourWorldPosition = default;

		if (!VoxelTraversalUtility.TryGetWorldHeight(p_worldData, p_position, ToExitDirection(p_direction), p_voxelRegistry, out float currentHeight))
		{
			return false;
		}

		Vector2Int columnOffset = ToColumnOffset(p_direction);
		int targetX = p_position.x + columnOffset.x;
		int targetZ = p_position.z + columnOffset.y;
		TraversalCandidate? bestCandidate = null;

		for (int y = 0; y < ChunkData.FixedSizeY - 2; y++)
		{
			Vector3Int candidatePosition = new Vector3Int(targetX, y, targetZ);
			if (!p_graphCache.HasNode(p_worldData, p_voxelRegistry, candidatePosition))
			{
				continue;
			}

			if (!VoxelTraversalUtility.TryGetWorldHeight(p_worldData, candidatePosition, ToEntryDirection(p_direction), p_voxelRegistry, out float nextHeight))
			{
				continue;
			}

			float verticalGap = Mathf.Abs(nextHeight - currentHeight);
			if (verticalGap > VoxelTraversalUtility.MaximumVerticalTraversalGap)
			{
				continue;
			}

			TraversalCandidate candidate = new TraversalCandidate(candidatePosition, verticalGap, Mathf.Abs(candidatePosition.y - p_position.y));
			if (!bestCandidate.HasValue ||
				candidate.VerticalGap < bestCandidate.Value.VerticalGap ||
				(Mathf.Approximately(candidate.VerticalGap, bestCandidate.Value.VerticalGap) &&
				 candidate.VerticalCellGap < bestCandidate.Value.VerticalCellGap))
			{
				bestCandidate = candidate;
			}
		}

		if (!bestCandidate.HasValue)
		{
			return false;
		}

		p_neighbourWorldPosition = bestCandidate.Value.Position;
		return true;
	}

	private static float Heuristic(Vector3Int p_a, Vector3Int p_b)
	{
		return Mathf.Abs(p_a.x - p_b.x) + Mathf.Abs(p_a.y - p_b.y) + Mathf.Abs(p_a.z - p_b.z);
	}

	private static float GetScore(Dictionary<Vector3Int, float> p_scores, Vector3Int p_position)
	{
		return p_scores.TryGetValue(p_position, out float score) ? score : float.PositiveInfinity;
	}

	private static void BuildPath(Dictionary<Vector3Int, Vector3Int> p_cameFrom, Vector3Int p_current, List<Vector3Int> p_path)
	{
		p_path.Clear();
		p_path.Add(p_current);

		while (p_cameFrom.TryGetValue(p_current, out Vector3Int previous))
		{
			p_current = previous;
			p_path.Add(p_current);
		}

		p_path.Reverse();
	}

	private static Vector2Int ToColumnOffset(VoxelTraversalGraph.Node.Direction p_direction)
	{
		return p_direction switch
		{
			VoxelTraversalGraph.Node.Direction.PositiveX => Vector2Int.right,
			VoxelTraversalGraph.Node.Direction.NegativeX => Vector2Int.left,
			VoxelTraversalGraph.Node.Direction.PositiveZ => Vector2Int.up,
			VoxelTraversalGraph.Node.Direction.NegativeZ => Vector2Int.down,
			_ => Vector2Int.zero
		};
	}

	private static CardinalHeightSet.Direction ToExitDirection(VoxelTraversalGraph.Node.Direction p_direction)
	{
		return p_direction switch
		{
			VoxelTraversalGraph.Node.Direction.PositiveX => CardinalHeightSet.Direction.PositiveX,
			VoxelTraversalGraph.Node.Direction.NegativeX => CardinalHeightSet.Direction.NegativeX,
			VoxelTraversalGraph.Node.Direction.PositiveZ => CardinalHeightSet.Direction.PositiveZ,
			VoxelTraversalGraph.Node.Direction.NegativeZ => CardinalHeightSet.Direction.NegativeZ,
			_ => CardinalHeightSet.Direction.Stationary
		};
	}

	private static CardinalHeightSet.Direction ToEntryDirection(VoxelTraversalGraph.Node.Direction p_direction)
	{
		return p_direction switch
		{
			VoxelTraversalGraph.Node.Direction.PositiveX => CardinalHeightSet.Direction.NegativeX,
			VoxelTraversalGraph.Node.Direction.NegativeX => CardinalHeightSet.Direction.PositiveX,
			VoxelTraversalGraph.Node.Direction.PositiveZ => CardinalHeightSet.Direction.NegativeZ,
			VoxelTraversalGraph.Node.Direction.NegativeZ => CardinalHeightSet.Direction.PositiveZ,
			_ => CardinalHeightSet.Direction.Stationary
		};
	}
}
