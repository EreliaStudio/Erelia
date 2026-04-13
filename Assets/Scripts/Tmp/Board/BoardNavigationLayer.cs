using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BoardNavigationLayer
{
	[NonSerialized]
	private BattleCellGraph _graph;

	public BattleCellGraph Graph => _graph;

	public IReadOnlyList<BattleCellGraph.Node> Nodes =>
		_graph != null ? _graph.AllNodes : Array.Empty<BattleCellGraph.Node>();

	public void Clear()
	{
		_graph = null;
	}

	public void Rebuild(BoardTerrainLayer p_terrainLayer)
	{
		if (p_terrainLayer == null || p_terrainLayer.VoxelRegistry == null)
		{
			_graph = null;
			return;
		}

		_graph = BattleCellGraphBuilder.Build(p_terrainLayer, p_terrainLayer.VoxelRegistry);
	}

	public bool HasNode(Vector3Int p_position)
	{
		return _graph != null && _graph.ContainsNode(p_position);
	}

	public bool TryGetNode(Vector3Int p_position, out BattleCellGraph.Node p_node)
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