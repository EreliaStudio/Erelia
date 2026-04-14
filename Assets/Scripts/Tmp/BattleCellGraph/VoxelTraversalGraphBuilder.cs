using System;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelTraversalGraphBuilder
{
	private readonly struct TraversalCandidate
	{
		public readonly VoxelTraversalGraph.Node Node;
		public readonly float VerticalGap;
		public readonly int VerticalCellGap;

		public TraversalCandidate(VoxelTraversalGraph.Node p_node, float p_verticalGap, int p_verticalCellGap)
		{
			Node = p_node;
			VerticalGap = p_verticalGap;
			VerticalCellGap = p_verticalCellGap;
		}
	}

	public static VoxelTraversalGraph Build(VoxelGrid p_grid, VoxelRegistry p_voxelRegistry)
	{
		if (p_grid == null)
		{
			throw new ArgumentNullException(nameof(p_grid));
		}

		VoxelTraversalGraph graph = new VoxelTraversalGraph(p_grid.SizeX, p_grid.SizeY, p_grid.SizeZ);

		if (p_voxelRegistry == null)
		{
			return graph;
		}

		Dictionary<Vector2Int, List<Vector3Int>> nodesByColumn = new Dictionary<Vector2Int, List<Vector3Int>>();

		for (int x = 0; x < p_grid.SizeX; x++)
		{
			for (int y = 0; y < p_grid.SizeY - 2; y++)
			{
				for (int z = 0; z < p_grid.SizeZ; z++)
				{
					Vector3Int position = new Vector3Int(x, y, z);
					if (!VoxelTraversalUtility.IsReachableCell(p_grid, position, p_voxelRegistry))
					{
						continue;
					}

					graph.CreateNode(position);

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

		IReadOnlyList<VoxelTraversalGraph.Node> allNodes = graph.AllNodes;
		for (int index = 0; index < allNodes.Count; index++)
		{
			VoxelTraversalGraph.Node node = allNodes[index];

			AssignNeighbour(node, Vector2Int.right, VoxelTraversalGraph.Node.Direction.PositiveX, CardinalHeightSet.Direction.PositiveX, CardinalHeightSet.Direction.NegativeX, graph, nodesByColumn, p_grid, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.left, VoxelTraversalGraph.Node.Direction.NegativeX, CardinalHeightSet.Direction.NegativeX, CardinalHeightSet.Direction.PositiveX, graph, nodesByColumn, p_grid, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.up, VoxelTraversalGraph.Node.Direction.PositiveZ, CardinalHeightSet.Direction.PositiveZ, CardinalHeightSet.Direction.NegativeZ, graph, nodesByColumn, p_grid, p_voxelRegistry);
			AssignNeighbour(node, Vector2Int.down, VoxelTraversalGraph.Node.Direction.NegativeZ, CardinalHeightSet.Direction.NegativeZ, CardinalHeightSet.Direction.PositiveZ, graph, nodesByColumn, p_grid, p_voxelRegistry);
		}

		return graph;
	}

	private static void AssignNeighbour(
		VoxelTraversalGraph.Node p_node,
		Vector2Int p_columnOffset,
		VoxelTraversalGraph.Node.Direction p_graphDirection,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		VoxelTraversalGraph p_graph,
		Dictionary<Vector2Int, List<Vector3Int>> p_nodesByColumn,
		VoxelGrid p_grid,
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
			Vector3Int candidatePosition = candidates[index];

			if (!TryGetVerticalGap(p_node.Position, candidatePosition, p_exitDirection, p_entryDirection, p_grid, p_voxelRegistry, out float verticalGap))
			{
				continue;
			}

			VoxelTraversalGraph.Node candidateNode = p_graph.GetNode(candidatePosition);

			TraversalCandidate candidateScore = new TraversalCandidate(
				candidateNode,
				verticalGap,
				Math.Abs(candidatePosition.y - p_node.Position.y));

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

		p_node.SetNeighbour(p_graphDirection, bestCandidate.Value.Node);
	}

	private static bool TryGetVerticalGap(
		Vector3Int p_current,
		Vector3Int p_next,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		VoxelGrid p_grid,
		VoxelRegistry p_voxelRegistry,
		out float p_verticalGap)
	{
		p_verticalGap = float.PositiveInfinity;

		if (!VoxelTraversalUtility.TryGetWorldHeight(p_grid, p_current, p_exitDirection, p_voxelRegistry, out float currentHeight) ||
			!VoxelTraversalUtility.TryGetWorldHeight(p_grid, p_next, p_entryDirection, p_voxelRegistry, out float nextHeight))
		{
			return false;
		}

		p_verticalGap = Mathf.Abs(nextHeight - currentHeight);
		return p_verticalGap <= VoxelTraversalUtility.MaximumVerticalTraversalGap;
	}
}
