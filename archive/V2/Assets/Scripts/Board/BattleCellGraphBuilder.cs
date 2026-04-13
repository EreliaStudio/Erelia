using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleCellGraphBuilder
{
	private const float MaximumVerticalTraversalGap = 0.5f;

	private readonly struct CardinalHeightKey : IEquatable<CardinalHeightKey>
	{
		private readonly CardinalHeightSet source;
		private readonly VoxelOrientation orientation;

		public CardinalHeightKey(CardinalHeightSet source, VoxelOrientation orientation)
		{
			this.source = source;
			this.orientation = orientation;
		}

		public bool Equals(CardinalHeightKey other)
		{
			return ReferenceEquals(source, other.source) && orientation == other.orientation;
		}

		public override bool Equals(object obj)
		{
			return obj is CardinalHeightKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			int hash = source != null ? source.GetHashCode() : 0;
			unchecked
			{
				hash = (hash * 397) ^ (int)orientation;
			}

			return hash;
		}
	}

	private readonly struct TraversalCandidate
	{
		public readonly BattleCellGraph.Node Node;
		public readonly float VerticalGap;
		public readonly int VerticalCellGap;

		public TraversalCandidate(BattleCellGraph.Node node, float verticalGap, int verticalCellGap)
		{
			Node = node;
			VerticalGap = verticalGap;
			VerticalCellGap = verticalCellGap;
		}
	}

	private static readonly Dictionary<CardinalHeightKey, CardinalHeightSet> CardinalHeightCache = new Dictionary<CardinalHeightKey, CardinalHeightSet>();

	public static BattleCellGraph Build(Board p_board, VoxelRegistry p_voxelRegistry)
	{
		BattleCellGraph graph = new BattleCellGraph();
		if (p_board == null || p_voxelRegistry == null)
		{
			return graph;
		}

		Dictionary<Vector2Int, List<Vector3Int>> nodesByColumn = new Dictionary<Vector2Int, List<Vector3Int>>();

		for (int x = 0; x < p_board.SizeX; x++)
		{
			for (int y = 0; y < p_board.SizeY - 2; y++)
			{
				for (int z = 0; z < p_board.SizeZ; z++)
				{
					Vector3Int position = new Vector3Int(x, y, z);
					if (!IsReachableCell(p_board, position, p_voxelRegistry))
					{
						continue;
					}

					graph.Nodes[position] = new BattleCellGraph.Node(position);

					Vector2Int column = new Vector2Int(x, z);
					if (!nodesByColumn.TryGetValue(column, out List<Vector3Int> columnNodes))
					{
						columnNodes = new List<Vector3Int>();
						nodesByColumn[column] = columnNodes;
					}

					columnNodes.Add(position);
				}
			}
		}

		foreach (BattleCellGraph.Node node in graph.Nodes.Values)
		{
			AssignNeighbour(node, Vector2Int.right, CardinalHeightSet.Direction.PositiveX, CardinalHeightSet.Direction.NegativeX, graph, nodesByColumn, p_board, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.left, CardinalHeightSet.Direction.NegativeX, CardinalHeightSet.Direction.PositiveX, graph, nodesByColumn, p_board, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.up, CardinalHeightSet.Direction.PositiveZ, CardinalHeightSet.Direction.NegativeZ, graph, nodesByColumn, p_board, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.down, CardinalHeightSet.Direction.NegativeZ, CardinalHeightSet.Direction.PositiveZ, graph, nodesByColumn, p_board, p_voxelRegistry);
		}

		return graph;
	}

	private static void AssignNeighbour(
		BattleCellGraph.Node p_node,
		Vector2Int p_columnOffset,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		BattleCellGraph p_graph,
		Dictionary<Vector2Int, List<Vector3Int>> p_nodesByColumn,
		Board p_board,
		VoxelRegistry p_voxelRegistry)
	{
		Vector2Int nextColumn = new Vector2Int(
			p_node.Position.x + p_columnOffset.x,
			p_node.Position.z + p_columnOffset.y);

		if (!p_nodesByColumn.TryGetValue(nextColumn, out List<Vector3Int> candidates) || candidates == null)
		{
			return;
		}

		TraversalCandidate? bestCandidate = null;
		for (int index = 0; index < candidates.Count; index++)
		{
			Vector3Int candidate = candidates[index];
			if (!TryGetVerticalGap(p_node.Position, candidate, p_exitDirection, p_entryDirection, p_board, p_voxelRegistry, out float verticalGap))
			{
				continue;
			}

			TraversalCandidate candidateScore = new TraversalCandidate(
				p_graph.Nodes[candidate],
				verticalGap,
				Math.Abs(candidate.y - p_node.Position.y));

			if (!bestCandidate.HasValue ||
				candidateScore.VerticalGap < bestCandidate.Value.VerticalGap ||
				(Mathf.Approximately(candidateScore.VerticalGap, bestCandidate.Value.VerticalGap) &&
				 candidateScore.VerticalCellGap < bestCandidate.Value.VerticalCellGap))
			{
				bestCandidate = candidateScore;
			}
		}

		if (!bestCandidate.HasValue)
		{
			return;
		}

		p_node.SetNeighbour(BattleCellGraph.Node.FromCardinalDirection(p_exitDirection), bestCandidate.Value.Node);
	}

	private static bool TryGetVerticalGap(
		Vector3Int p_current,
		Vector3Int p_next,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		Board p_board,
		VoxelRegistry p_voxelRegistry,
		out float p_verticalGap)
	{
		p_verticalGap = float.PositiveInfinity;

		if (!TryGetWorldHeight(p_board, p_current, p_exitDirection, p_voxelRegistry, out float currentHeight) ||
			!TryGetWorldHeight(p_board, p_next, p_entryDirection, p_voxelRegistry, out float nextHeight))
		{
			return false;
		}

		p_verticalGap = Mathf.Abs(nextHeight - currentHeight);
		return p_verticalGap <= MaximumVerticalTraversalGap;
	}

	private static bool TryGetWorldHeight(
		Board p_board,
		Vector3Int p_position,
		CardinalHeightSet.Direction p_direction,
		VoxelRegistry p_voxelRegistry,
		out float p_height)
	{
		p_height = 0f;

		if (!p_board.IsInside(p_position))
		{
			return false;
		}

		VoxelCell cell = p_board.Cells[p_position.x, p_position.y, p_position.z];
		if (cell == null || cell.IsEmpty || !p_voxelRegistry.TryGetVoxel(cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition?.Shape == null)
		{
			return false;
		}

		CardinalHeightSet localHeights = voxelDefinition.Shape.GetCardinalHeights(cell.FlipOrientation);
		CardinalHeightSet worldHeights = ResolveWorldHeights(localHeights, cell.Orientation);
		p_height = p_position.y + worldHeights.Get(p_direction);
		return true;
	}

	private static CardinalHeightSet ResolveWorldHeights(CardinalHeightSet p_source, VoxelOrientation p_orientation)
	{
		if (p_source == null)
		{
			return CardinalHeightSet.CreateDefault();
		}

		CardinalHeightKey key = new CardinalHeightKey(p_source, p_orientation);
		if (CardinalHeightCache.TryGetValue(key, out CardinalHeightSet cached))
		{
			return cached;
		}

		CardinalHeightSet transformed = new CardinalHeightSet(
			positiveX: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.PositiveX, p_orientation)),
			negativeX: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.NegativeX, p_orientation)),
			positiveZ: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.PositiveZ, p_orientation)),
			negativeZ: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.NegativeZ, p_orientation)),
			stationary: p_source.Stationary);

		CardinalHeightCache[key] = transformed;
		return transformed;
	}

	private static CardinalHeightSet.Direction ResolveLocalDirection(CardinalHeightSet.Direction p_worldDirection, VoxelOrientation p_orientation)
	{
		switch (p_orientation)
		{
			case VoxelOrientation.PositiveX:
				return p_worldDirection;
			case VoxelOrientation.PositiveZ:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.PositiveZ,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.NegativeX,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.PositiveX,
					_ => CardinalHeightSet.Direction.Stationary
				};
			case VoxelOrientation.NegativeX:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.NegativeX,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.PositiveX,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.PositiveZ,
					_ => CardinalHeightSet.Direction.Stationary
				};
			case VoxelOrientation.NegativeZ:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.PositiveZ,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.PositiveX,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.NegativeX,
					_ => CardinalHeightSet.Direction.Stationary
				};
			default:
				return p_worldDirection;
		}
	}

	private static bool IsReachableCell(Board p_board, Vector3Int p_position, VoxelRegistry p_voxelRegistry)
	{
		return IsWalkable(p_board.Cells[p_position.x, p_position.y, p_position.z], p_voxelRegistry) &&
			IsAirOrWalkable(p_board.Cells[p_position.x, p_position.y + 1, p_position.z], p_voxelRegistry) &&
			IsAirOrWalkable(p_board.Cells[p_position.x, p_position.y + 2, p_position.z], p_voxelRegistry);
	}

	private static bool IsAirOrWalkable(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		return p_cell == null || p_cell.IsEmpty || IsWalkable(p_cell, p_voxelRegistry);
	}

	private static bool IsWalkable(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		if (p_cell == null || p_cell.IsEmpty)
		{
			return false;
		}

		if (!p_voxelRegistry.TryGetVoxel(p_cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition == null)
		{
			return false;
		}

		return voxelDefinition.Data != null && voxelDefinition.Data.Traversal == VoxelTraversal.Walkable;
	}
}
