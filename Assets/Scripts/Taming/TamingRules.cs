using System.Collections.Generic;

public static class TamingRules
{
	public static bool CanTrackTaming(BattleUnit p_unit, TamingProfile p_profile)
	{
		if (p_unit == null ||
			p_unit.Side != BattleSide.Enemy ||
			p_unit.SourceUnit?.Species == null ||
			p_profile == null ||
			!p_profile.HasConditions)
		{
			return false;
		}

		return true;
	}

	public static bool AreAllConditionsComplete(
		TamingProfile p_profile,
		IReadOnlyList<FeatRequirement.Advancement> p_advancements)
	{
		if (p_profile?.Conditions == null ||
			p_profile.Conditions.Count == 0 ||
			p_advancements == null ||
			p_advancements.Count < p_profile.Conditions.Count)
		{
			return false;
		}

		for (int index = 0; index < p_profile.Conditions.Count; index++)
		{
			FeatRequirement condition = p_profile.Conditions[index];
			if (condition == null)
			{
				return false;
			}

			if (!condition.IsCompleted(p_advancements[index]))
			{
				return false;
			}
		}

		return true;
	}

	public static CreatureUnit CreateRecruitFromImpressedUnit(BattleUnit p_impressedUnit)
	{
		CreatureUnit sourceUnit = p_impressedUnit?.SourceUnit;
		if (sourceUnit?.Species == null)
		{
			return null;
		}

		CreatureUnit recruit = new CreatureUnit
		{
			Species = sourceUnit.Species,
			FeatBoardProgress = new FeatBoardProgress()
		};

		FeatProgressionService.ApplyProgress(recruit);
		recruit.CurrentFormID = sourceUnit.CurrentFormID;
		return recruit;
	}
}