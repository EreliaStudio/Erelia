using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BoardNavigationLayer
{
	[NonSerialized]
	private VoxelTraversalGraph _graph;

	public VoxelTraversalGraph Graph => _graph;

	public IReadOnlyList<VoxelTraversalGraph.Node> Nodes =>
		_graph != null ? _graph.AllNodes : Array.Empty<VoxelTraversalGraph.Node>();

	public void Clear()
	{
		_graph = null;
	}

	public void Rebuild(BoardTerrainLayer p_terrainLayer, HashSet<Vector2Int> p_excludedColumns = null)
	{
		if (p_terrainLayer == null || p_terrainLayer.VoxelRegistry == null)
		{
			_graph = null;
			return;
		}

		_graph = VoxelTraversalGraphBuilder.Build(p_terrainLayer, p_terrainLayer.VoxelRegistry, p_excludedColumns);
	}

	public bool HasNode(Vector3Int p_position)
	{
		return _graph != null && _graph.ContainsNode(p_position);
	}

	public bool TryGetNode(Vector3Int p_position, out VoxelTraversalGraph.Node p_node)
	{
		if (_graph == null)
		{
			p_node = null;
			return false;
		}

		return _graph.TryGetNode(p_position, out p_node);
	}

	public bool IsStandable(Vector3Int p_position)
	{
		return HasNode(p_position);
	}
}
