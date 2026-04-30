using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FeatNodeProgress
{
	public string NodeId = string.Empty;
	public int CompletionCount = 0;
	public List<FeatRequirementProgress> RequirementProgress = new List<FeatRequirementProgress>();

	public FeatNodeProgress(FeatNode p_node)
	{
		NodeId = p_node != null ? p_node.Id : string.Empty;
		RequirementProgress = new List<FeatRequirementProgress>();

		if (p_node?.Requirements == null)
		{
			return;
		}

		for (int index = 0; index < p_node.Requirements.Count; index++)
		{
			RequirementProgress.Add(new FeatRequirementProgress
			{
				Requirement = p_node.Requirements[index],
				CurrentProgress = 0f
			});
		}
	}

	public bool HasRequirements => RequirementProgress.Count > 0;
	public bool IsCompleted => HasRequirements && RequirementProgress.All(p_requirement => p_requirement != null && p_requirement.IsCompleted);

	public bool IsExhausted(FeatNode p_node)
	{
		if (p_node == null)
		{
			return true;
		}

		if (p_node.NumberOfRepeatTime < 0)
		{
			return false;
		}

		int maxCompletionCount = Math.Max(1, p_node.NumberOfRepeatTime);
		return CompletionCount >= maxCompletionCount;
	}

	public void RegisterEvents(
		IReadOnlyList<FeatRequirement.EventBase> p_featEvents,
		bool p_includeTransientRequirements = true)
	{
		foreach (FeatRequirementProgress requirement in RequirementProgress)
		{
			if (requirement == null)
			{
				continue;
			}

			requirement.RegisterEvents(p_featEvents, p_includeTransientRequirements);
		}
	}

	public void ResetRequirementProgress()
	{
		foreach (FeatRequirementProgress requirement in RequirementProgress)
		{
			if (requirement == null)
			{
				continue;
			}

			requirement.CurrentProgress = 0f;
			requirement.CompletedRepeatCount = 0;
		}
	}

	public void ResetTransientRequirementProgress()
	{
		foreach (FeatRequirementProgress requirement in RequirementProgress)
		{
			if (requirement == null || requirement.PersistsAcrossFights)
			{
				continue;
			}

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
	public FeatRequirement.Advancement Advancement = new FeatRequirement.Advancement(0f, 0);

	public float CurrentProgress
	{
		get => Advancement.Progress;
		set => Advancement.Progress = value;
	}

	public int CompletedRepeatCount
	{
		get => Advancement.CompletedRepeatCount;
		set => Advancement.CompletedRepeatCount = value;
	}

	public bool IsCompleted
	{
		get
		{
			if (Requirement == null)
			{
				return false;
			}

			return Requirement.IsCompleted(Advancement);
		}
	}

	public bool PersistsAcrossFights
	{
		get
		{
			return Requirement != null && Requirement.RequirementScope == FeatRequirement.Scope.Game;
		}
	}

	public void RegisterEvents(
		IReadOnlyList<FeatRequirement.EventBase> p_featEvents,
		bool p_includeTransientRequirements = true)
	{
		if (Requirement == null)
		{
			return;
		}

		if (p_includeTransientRequirements == false && PersistsAcrossFights == false)
		{
			return;
		}

		Advancement = Requirement.EvaluateEvents(p_featEvents, Advancement);
	}
}
