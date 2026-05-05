using System;
using System.Collections.Generic;

[Serializable]
public sealed class TamingProgress
{
	private readonly List<FeatRequirement.Advancement> conditionAdvancements = new List<FeatRequirement.Advancement>();

	public BattleUnit TargetUnit { get; }
	public TamingProfile Profile { get; }
	public bool IsImpressed { get; private set; }
	public bool HasFailed { get; private set; }

	public IReadOnlyList<FeatRequirement.Advancement> ConditionAdvancements => conditionAdvancements;

	public TamingProgress(BattleUnit p_targetUnit, TamingProfile p_profile)
	{
		TargetUnit = p_targetUnit;
		Profile = p_profile;

		int conditionCount = p_profile?.Conditions?.Count ?? 0;
		for (int index = 0; index < conditionCount; index++)
		{
			conditionAdvancements.Add(default);
		}
	}

	public void EvaluateEvents(IReadOnlyList<FeatRequirement.EventBase> p_events)
	{
		if (IsImpressed || HasFailed || Profile?.Conditions == null)
		{
			return;
		}

		for (int index = 0; index < Profile.Conditions.Count; index++)
		{
			FeatRequirement condition = Profile.Conditions[index];
			if (condition == null)
			{
				continue;
			}

			FeatRequirement.Advancement currentAdvancement = conditionAdvancements[index];
			conditionAdvancements[index] = condition.EvaluateEvents(p_events, currentAdvancement);
		}

		if (TamingRules.AreAllConditionsComplete(Profile, conditionAdvancements))
		{
			IsImpressed = true;
		}
	}

	public void MarkFailed()
	{
		if (IsImpressed)
		{
			return;
		}

		HasFailed = true;
	}

	public void Reset()
	{
		IsImpressed = false;
		HasFailed = false;

		for (int index = 0; index < conditionAdvancements.Count; index++)
		{
			conditionAdvancements[index] = default;
		}
	}
}