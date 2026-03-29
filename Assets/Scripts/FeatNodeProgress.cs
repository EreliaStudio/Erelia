using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FeatNodeProgress
{
	public FeatNode Node;
	public int UnlockCount = 0;
	public List<FeatRequirementProgress> RequirementProgress = new List<FeatRequirementProgress>();

	public bool IsCompleted => RequirementProgress.All(p_requirement => p_requirement.IsCompleted);

	public void Register(FeatRequirement.EventBase featEvent)
	{
		foreach (var requirement in RequirementProgress)
		{
			requirement.Register(featEvent);
		}
	}
}

[Serializable]
public class FeatRequirementProgress
{
	public FeatRequirement Requirement;
	public float CurrentProgress = 0f;
	public bool IsCompleted => CurrentProgress >= 100.0f;

	public void Register(FeatRequirement.EventBase featEvent)
	{
		CurrentProgress += Requirement.Register(featEvent);
	}
}
