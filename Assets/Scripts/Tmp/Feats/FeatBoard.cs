using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FeatBoard
{
	[SerializeReference]
	public List<FeatNode> Nodes = new List<FeatNode>();

	public string RootNodeId = string.Empty;

	public FeatNode GetNode(string p_nodeId)
	{
		if (string.IsNullOrWhiteSpace(p_nodeId))
		{
			return null;
		}

		for (int index = 0; index < Nodes.Count; index++)
		{
			FeatNode node = Nodes[index];
			if (node != null && string.Equals(node.Id, p_nodeId, StringComparison.Ordinal))
			{
				return node;
			}
		}

		return null;
	}

	public FeatNode GetRootNode()
	{
		return GetNode(RootNodeId);
	}

	public bool IsRootNode(FeatNode p_node)
	{
		return p_node != null && string.Equals(RootNodeId, p_node.Id, StringComparison.Ordinal);
	}

	public List<FeatNode> GetNeighbourNodes(FeatNode p_node)
	{
		List<FeatNode> neighbours = new List<FeatNode>();
		if (p_node == null || p_node.NeighbourNodeIds == null)
		{
			return neighbours;
		}

		HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
		for (int index = 0; index < p_node.NeighbourNodeIds.Count; index++)
		{
			string neighbourId = p_node.NeighbourNodeIds[index];
			if (string.IsNullOrWhiteSpace(neighbourId) || !seenIds.Add(neighbourId))
			{
				continue;
			}

			FeatNode neighbour = GetNode(neighbourId);
			if (neighbour != null)
			{
				neighbours.Add(neighbour);
			}
		}

		return neighbours;
	}
}
