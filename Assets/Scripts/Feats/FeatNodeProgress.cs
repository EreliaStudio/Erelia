using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FeatNodeProgress
{
	public FeatNode Node;
	public int CompletionCount = 0;
	public List<FeatRequirementProgress> RequirementProgress = new List<FeatRequirementProgress>();

	public bool IsCompleted => RequirementProgress.All(p_requirement => p_requirement.IsCompleted);

	public bool IsExhausted
	{
		get
		{
			if (Node == null)
			{
				return true;
			}

			if (Node.NumberOfRepeatTime < 0)
			{
				return false;
			}

			int maxCompletionCount = Math.Max(1, Node.NumberOfRepeatTime);
			return CompletionCount >= maxCompletionCount;
		}
	}

	public void Register(FeatRequirement.EventBase p_featEvent)
	{
		foreach (FeatRequirementProgress requirement in RequirementProgress)
		{
			requirement.Register(p_featEvent);
		}
	}

	public void ResetRequirementProgress()
	{
		foreach (FeatRequirementProgress requirement in RequirementProgress)
		{
			requirement.CurrentProgress = 0f;
		}
	}

	public void Complete()
	{
		CompletionCount++;
		ResetRequirementProgress();
	}
}

[Serializable]
public class FeatRequirementProgress
{
	public FeatRequirement Requirement;
	public float CurrentProgress = 0f;

	public bool IsCompleted => CurrentProgress >= 100.0f;

	public void Register(FeatRequirement.EventBase p_featEvent)
	{
		CurrentProgress += Requirement.Register(p_featEvent);
	}
}