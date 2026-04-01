using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FeatBoardProgress
{
	public List<FeatNodeProgress> NodeProgress = new List<FeatNodeProgress>();

	public void Register(FeatRequirement.EventBase p_featEvent)
	{
		foreach (FeatNodeProgress nodeProgress in NodeProgress)
		{
			if (nodeProgress == null)
			{
				continue;
			}

			nodeProgress.Register(p_featEvent);
		}
	}

	public void ApplyCompletedNode(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			return;
		}

		foreach (FeatNodeProgress nodeProgress in NodeProgress)
		{
			if (nodeProgress == null)
			{
				continue;
			}

			FeatNode node = p_creatureUnit.ResolveNode(nodeProgress.NodeId);
			if (node == null)
			{
				continue;
			}

			if (nodeProgress.IsCompleted == false)
			{
				continue;
			}

			if (node.Rewards != null)
			{
				foreach (FeatReward reward in node.Rewards)
				{
					if (reward == null)
					{
						continue;
					}

					reward.Apply(p_creatureUnit);
				}
			}

			nodeProgress.Complete();
		}
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

		nodeProgress = CreateProgress(p_node);
		NodeProgress.Add(nodeProgress);
		return nodeProgress;
	}

	private static FeatNodeProgress CreateProgress(FeatNode p_node)
	{
		FeatNodeProgress nodeProgress = new FeatNodeProgress
		{
			NodeId = p_node != null ? p_node.Id : string.Empty,
			CompletionCount = 0,
			RequirementProgress = new List<FeatRequirementProgress>()
		};

		if (p_node != null && p_node.Requirements != null)
		{
			foreach (FeatRequirement requirement in p_node.Requirements)
			{
				nodeProgress.RequirementProgress.Add(new FeatRequirementProgress
				{
					Requirement = requirement,
					CurrentProgress = 0f
				});
			}
		}

		return nodeProgress;
	}
}