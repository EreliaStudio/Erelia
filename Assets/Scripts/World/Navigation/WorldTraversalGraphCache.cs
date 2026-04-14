using System.Collections.Generic;
using UnityEngine;

public sealed class WorldTraversalGraphCache
{
	private readonly Dictionary<ChunkCoordinates, VoxelTraversalGraph> graphs = new Dictionary<ChunkCoordinates, VoxelTraversalGraph>();

	public void Clear()
	{
		graphs.Clear();
	}

	public void Invalidate(ChunkCoordinates p_coordinates)
	{
		graphs.Remove(p_coordinates);
	}

	public bool TryGetGraph(WorldData p_worldData, VoxelRegistry p_voxelRegistry, ChunkCoordinates p_coordinates, out VoxelTraversalGraph p_graph)
	{
		if (graphs.TryGetValue(p_coordinates, out p_graph) && p_graph != null)
		{
			return true;
		}

		if (p_worldData == null || p_voxelRegistry == null || !p_worldData.TryGetChunk(p_coordinates, out ChunkData chunkData) || chunkData == null)
		{
			p_graph = null;
			return false;
		}

		p_graph = VoxelTraversalGraphBuilder.Build(chunkData, p_voxelRegistry);
		graphs[p_coordinates] = p_graph;
		return true;
	}

	public bool TryGetNode(WorldData p_worldData, VoxelRegistry p_voxelRegistry, Vector3Int p_worldPosition, out VoxelTraversalGraph.Node p_node)
	{
		p_node = null;

		if (p_worldData == null || p_voxelRegistry == null || !p_worldData.TryGetChunk(p_worldPosition, out ChunkCoordinates chunkCoordinates, out Vector3Int localPosition, out _))
		{
			return false;
		}

		return TryGetGraph(p_worldData, p_voxelRegistry, chunkCoordinates, out VoxelTraversalGraph graph) &&
			   graph.TryGetNode(localPosition, out p_node);
	}

	public bool HasNode(WorldData p_worldData, VoxelRegistry p_voxelRegistry, Vector3Int p_worldPosition)
	{
		return TryGetNode(p_worldData, p_voxelRegistry, p_worldPosition, out _);
	}
}
