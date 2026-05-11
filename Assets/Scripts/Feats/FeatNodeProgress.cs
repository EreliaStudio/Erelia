using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

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
		IReadOnlyList<BattleEvent> p_featEvents,
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

	public JObject ToJson()
	{
		JArray requirements = new JArray();
		for (int index = 0; index < RequirementProgress.Count; index++)
		{
			FeatRequirementProgress req = RequirementProgress[index];
			requirements.Add(req != null ? req.ToJson() : new JObject());
		}

		return new JObject
		{
			["nodeId"] = NodeId ?? string.Empty,
			["completions"] = CompletionCount,
			["requirements"] = requirements
		};
	}

	public static FeatNodeProgress FromJson(JObject p_json, FeatNode p_node)
	{
		FeatNodeProgress progress = new FeatNodeProgress(p_node);
		progress.CompletionCount = Math.Max(0, p_json["completions"]?.Value<int>() ?? 0);

		JArray requirements = p_json["requirements"] as JArray;
		if (requirements == null)
		{
			return progress;
		}

		int count = Math.Min(progress.RequirementProgress.Count, requirements.Count);
		for (int index = 0; index < count; index++)
		{
			if (requirements[index] is JObject reqJson)
			{
				progress.RequirementProgress[index].LoadFromJson(reqJson);
			}
		}

		return progress;
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
		IReadOnlyList<BattleEvent> p_featEvents,
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

	public JObject ToJson()
	{
		return new JObject
		{
			["progress"] = CurrentProgress,
			["repeats"] = CompletedRepeatCount
		};
	}

	public void LoadFromJson(JObject p_json)
	{
		if (p_json == null)
		{
			return;
		}

		CurrentProgress = p_json["progress"]?.Value<float>() ?? 0f;
		CompletedRepeatCount = Math.Max(0, p_json["repeats"]?.Value<int>() ?? 0);
	}
}
