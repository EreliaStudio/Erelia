using System.Collections.Generic;

public static class CreatureUnitFeatProgressUtility
{
	public static List<FeatNode> GetReachableNodes(CreatureUnit p_unit)
	{
		List<FeatNode> reachableNodes = new List<FeatNode>();

		if (p_unit == null || p_unit.Species == null || p_unit.Species.FeatBoard == null)
		{
			return reachableNodes;
		}

		FeatBoard featBoard = p_unit.Species.FeatBoard;
		if (featBoard.Nodes == null || featBoard.Nodes.Count == 0)
		{
			return reachableNodes;
		}

		for (int nodeIndex = 0; nodeIndex < featBoard.Nodes.Count; nodeIndex++)
		{
			FeatNode node = featBoard.Nodes[nodeIndex];
			if (node == null)
			{
				continue;
			}

			if (IsNodeReachable(p_unit, node))
			{
				reachableNodes.Add(node);
			}
		}

		return reachableNodes;
	}

	public static bool IsNodeReachable(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_unit.Species == null || p_unit.Species.FeatBoard == null || p_node == null)
		{
			return false;
		}

		FeatNodeProgress progress = FindNodeProgress(p_unit, p_node);
		if (progress != null && progress.IsExhausted(p_node))
		{
			return false;
		}

		if (p_node.Kind == FeatNodeKind.Form && IsFormNodeLockedByCurrentTier(p_unit, p_node))
		{
			return false;
		}

		if (p_unit.Species.FeatBoard.RootNode == p_node)
		{
			return true;
		}

		if (p_node.NeighbourNodes == null)
		{
			return false;
		}

		for (int neighbourIndex = 0; neighbourIndex < p_node.NeighbourNodes.Count; neighbourIndex++)
		{
			FeatNode neighbour = p_node.NeighbourNodes[neighbourIndex];
			if (neighbour == null)
			{
				continue;
			}

			if (GetCompletionCount(p_unit, neighbour) > 0)
			{
				return true;
			}
		}

		return false;
	}

	public static bool IsNodeCompleted(CreatureUnit p_unit, FeatNode p_node)
	{
		return GetCompletionCount(p_unit, p_node) > 0;
	}

	public static bool IsNodeExhausted(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatNodeProgress progress = FindNodeProgress(p_unit, p_node);
		return progress != null && progress.IsExhausted(p_node);
	}

	public static int GetCompletionCount(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatNodeProgress progress = FindNodeProgress(p_unit, p_node);
		return progress != null ? progress.CompletionCount : 0;
	}

	public static FeatNodeProgress FindNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_unit.FeatBoardProgress == null || p_unit.FeatBoardProgress.NodeProgress == null || p_node == null)
		{
			return null;
		}

		for (int index = 0; index < p_unit.FeatBoardProgress.NodeProgress.Count; index++)
		{
			FeatNodeProgress progress = p_unit.FeatBoardProgress.NodeProgress[index];
			if (progress != null && progress.NodeId == p_node.Id)
			{
				return progress;
			}
		}

		return null;
	}

	public static void CompleteNodeOnce(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_node == null)
		{
			return;
		}

		p_unit.EnsureInitialized();

		if (!IsNodeReachable(p_unit, p_node))
		{
			return;
		}

		FeatNodeProgress progress = p_unit.FeatBoardProgress.GetOrCreateProgress(p_node);
		if (progress == null)
		{
			return;
		}

		if (progress.IsExhausted(p_node))
		{
			return;
		}

		progress.CompletionCount++;
		progress.ResetRequirementProgress();

		p_unit.RebuildFromProgress();
	}

	public static void ResetNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_unit.FeatBoardProgress == null || p_unit.FeatBoardProgress.NodeProgress == null || p_node == null)
		{
			return;
		}

		p_unit.EnsureInitialized();

		if (p_unit.Species != null && p_unit.Species.FeatBoard != null && p_unit.Species.FeatBoard.RootNode == p_node)
		{
			FeatNodeProgress rootProgress = p_unit.FeatBoardProgress.GetOrCreateProgress(p_node);
			if (rootProgress != null)
			{
				rootProgress.CompletionCount = 1;
				rootProgress.ResetRequirementProgress();
			}

			p_unit.RebuildFromProgress();
			return;
		}

		for (int index = p_unit.FeatBoardProgress.NodeProgress.Count - 1; index >= 0; index--)
		{
			FeatNodeProgress progress = p_unit.FeatBoardProgress.NodeProgress[index];
			if (progress == null || progress.NodeId != p_node.Id)
			{
				continue;
			}

			p_unit.FeatBoardProgress.NodeProgress.RemoveAt(index);
		}

		p_unit.RebuildFromProgress();
	}

	public static void ClearAllProgress(CreatureUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		p_unit.ClearProgress();
	}

	public static int GetHighestUnlockedFormTier(CreatureUnit p_unit)
	{
		if (p_unit == null || p_unit.FeatBoardProgress == null || p_unit.FeatBoardProgress.NodeProgress == null)
		{
			return -1;
		}

		int highestTier = -1;

		for (int index = 0; index < p_unit.FeatBoardProgress.NodeProgress.Count; index++)
		{
			FeatNodeProgress progress = p_unit.FeatBoardProgress.NodeProgress[index];
			if (progress == null)
			{
				continue;
			}

			FeatNode node = p_unit.ResolveNode(progress.NodeId);
			if (node == null)
			{
				continue;
			}

			if (node.Kind != FeatNodeKind.Form)
			{
				continue;
			}

			if (progress.CompletionCount <= 0)
			{
				continue;
			}

			if (node.FormTier > highestTier)
			{
				highestTier = node.FormTier;
			}
		}

		return highestTier;
	}

	public static bool IsFormNodeLockedByCurrentTier(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_node == null || p_node.Rewards == null)
		{
			return false;
		}

		int currentTier = p_unit.GetForm().Tier;

		for (int i = 0; i < p_node.Rewards.Count; i++)
		{
			if (p_node.Rewards[i] is ChangeFormReward changeFormReward)
			{
				if (string.IsNullOrEmpty(changeFormReward.FormKey))
				{
					continue;
				}

				if (p_unit.Species.Forms.TryGetValue(changeFormReward.FormKey, out CreatureForm targetForm))
				{
					if (targetForm.Tier <= currentTier)
					{
						return true;
					}
				}
			}
		}

		return false;
	}
}