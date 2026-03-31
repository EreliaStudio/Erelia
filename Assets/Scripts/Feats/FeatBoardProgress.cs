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
			if (nodeProgress == null || nodeProgress.Node == null)
			{
				continue;
			}

			if (nodeProgress.IsExhausted)
			{
				continue;
			}

			nodeProgress.Register(p_featEvent);
		}
	}

	public void ApplyCompletedNode(CreatureUnit p_creatureUnit)
	{
		foreach (FeatNodeProgress nodeProgress in NodeProgress)
		{
			if (nodeProgress == null || nodeProgress.Node == null)
			{
				continue;
			}

			if (nodeProgress.IsCompleted == false)
			{
				continue;
			}

			foreach (FeatReward reward in nodeProgress.Node.Rewards)
			{
				reward.Apply(p_creatureUnit);
			}

			nodeProgress.Complete();
		}
	}

	public FeatNodeProgress GetOrCreateProgress(FeatNode p_node)
	{
		FeatNodeProgress nodeProgress = NodeProgress.FirstOrDefault(p_progress => p_progress != null && p_progress.Node == p_node);
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
			Node = p_node,
			CompletionCount = 0,
			RequirementProgress = new List<FeatRequirementProgress>()
		};

		if (p_node != null)
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