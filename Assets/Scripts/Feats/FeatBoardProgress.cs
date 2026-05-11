using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

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

	public JObject ToJson()
	{
		JArray nodes = new JArray();
		for (int index = 0; index < NodeProgress.Count; index++)
		{
			FeatNodeProgress nodeProgress = NodeProgress[index];
			if (nodeProgress != null)
			{
				nodes.Add(nodeProgress.ToJson());
			}
		}

		return new JObject { ["nodes"] = nodes };
	}

	public static FeatBoardProgress FromJson(JObject p_json, FeatBoard p_board)
	{
		FeatBoardProgress progress = new FeatBoardProgress();

		JArray nodes = p_json?["nodes"] as JArray;
		if (nodes == null || p_board == null)
		{
			return progress;
		}

		foreach (JObject nodeJson in nodes)
		{
			string nodeId = nodeJson["nodeId"]?.Value<string>();
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				continue;
			}

			FeatNode node = p_board.GetNode(nodeId);
			if (node == null)
			{
				continue;
			}

			progress.NodeProgress.Add(FeatNodeProgress.FromJson(nodeJson, node));
		}

		return progress;
	}
}
