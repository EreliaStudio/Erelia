using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FeatBoardProgress
{
	public List<FeatNodeProgress> NodeProgress = new List<FeatNodeProgress>();

	public FeatNodeProgress FindProgress(string p_nodeId)
	{
		return NodeProgress.FirstOrDefault(
			p_progress => p_progress != null && p_progress.NodeId == p_nodeId
		);
	}

	public FeatNodeProgress GetOrCreateProgress(FeatNode p_node)
	{
		if (p_node == null || string.IsNullOrEmpty(p_node.Id))
		{
			return null;
		}

		FeatNodeProgress nodeProgress = NodeProgress.FirstOrDefault(
			p_progress => p_progress != null && p_progress.NodeId == p_node.Id
		);

		if (nodeProgress != null)
		{
			return nodeProgress;
		}

		nodeProgress = new FeatNodeProgress(p_node);
		NodeProgress.Add(nodeProgress);
		return nodeProgress;
	}

	public void RemoveProgress(string p_nodeId)
	{
		if (NodeProgress == null || string.IsNullOrWhiteSpace(p_nodeId))
		{
			return;
		}

		for (int index = NodeProgress.Count - 1; index >= 0; index--)
		{
			FeatNodeProgress progress = NodeProgress[index];
			if (progress != null && progress.NodeId == p_nodeId)
			{
				NodeProgress.RemoveAt(index);
			}
		}
	}

}
