#if UNITY_EDITOR
using System;
using UnityEngine;

internal static class EncounterEditorUtility
{
	public static readonly string[] TierLabels =
	{
		"No Badge",
		"1 Badge",
		"2 Badges",
		"3 Badges",
		"4 Badges",
		"5 Badges",
		"6 Badges",
		"7 Badges",
		"8 Badges",
		"Post Game"
	};

	public static EncounterTier GetTier(EncounterTable encounterTable, int tierIndex)
	{
		if (encounterTable == null)
		{
			return null;
		}

		return tierIndex switch
		{
			0 => encounterTable.NoBadge,
			1 => encounterTable.OneBadge,
			2 => encounterTable.TwoBadges,
			3 => encounterTable.ThreeBadges,
			4 => encounterTable.FourBadges,
			5 => encounterTable.FiveBadges,
			6 => encounterTable.SixBadges,
			7 => encounterTable.SevenBadges,
			8 => encounterTable.EightBadges,
			_ => encounterTable.PostGame
		};
	}

	public static void EnsureRule(BiomeEncounterRule rule)
	{
		if (rule == null)
		{
			return;
		}

		rule.BoardConfigurations ??= new System.Collections.Generic.List<BoardConfiguration>();
		rule.EncounterTable ??= new EncounterTable();
		for (int tierIndex = 0; tierIndex < TierLabels.Length; tierIndex++)
		{
			EnsureTier(GetTier(rule.EncounterTable, tierIndex));
		}
	}

	public static void EnsureTier(EncounterTier tier)
	{
		if (tier?.WeightedTeams == null)
		{
			return;
		}

		for (int entryIndex = 0; entryIndex < tier.WeightedTeams.Count; entryIndex++)
		{
			EncounterTier.Entry entry = tier.WeightedTeams[entryIndex];
			if (entry == null)
			{
				tier.WeightedTeams[entryIndex] = CreateEntry(entryIndex);
				entry = tier.WeightedTeams[entryIndex];
			}

			EnsureEntry(entry);
		}
	}

	public static EncounterTier.Entry CreateEntry(int index)
	{
		var entry = new EncounterTier.Entry
		{
			DisplayName = $"team {index + 1}",
			Weight = 1,
			Team = new EncounterUnit[GameRule.TeamMemberCount]
		};

		EnsureEntry(entry);
		return entry;
	}

	public static void EnsureEntry(EncounterTier.Entry entry)
	{
		if (entry == null)
		{
			return;
		}

		entry.DisplayName ??= string.Empty;
		if (entry.Team == null || entry.Team.Length != GameRule.TeamMemberCount)
		{
			EncounterUnit[] resizedTeam = new EncounterUnit[GameRule.TeamMemberCount];
			if (entry.Team != null)
			{
				Array.Copy(entry.Team, resizedTeam, Mathf.Min(entry.Team.Length, resizedTeam.Length));
			}

			entry.Team = resizedTeam;
		}

		for (int unitIndex = 0; unitIndex < entry.Team.Length; unitIndex++)
		{
			entry.Team[unitIndex] ??= new EncounterUnit();
			entry.Team[unitIndex].Behaviour ??= new AIBehaviour();
		}
	}

	public static EncounterUnit GetOrCreateUnit(EncounterTier.Entry entry, int index)
	{
		if (entry == null)
		{
			return null;
		}

		EnsureEntry(entry);
		if (index < 0 || index >= entry.Team.Length)
		{
			return null;
		}

		entry.Team[index] ??= new EncounterUnit();
		return entry.Team[index];
	}

	public static string GetEntryDisplayName(EncounterTier.Entry entry, int fallbackIndex)
	{
		if (entry == null)
		{
			return $"team {fallbackIndex + 1}";
		}

		if (!string.IsNullOrWhiteSpace(entry.DisplayName))
		{
			return entry.DisplayName;
		}

		if (entry.Team != null)
		{
			for (int index = 0; index < entry.Team.Length; index++)
			{
				string unitLabel = GetUnitDisplayName(entry.Team[index]);
				if (unitLabel != "-----")
				{
					return unitLabel;
				}
			}
		}

		return $"team {fallbackIndex + 1}";
	}

	public static string GetUnitDisplayName(CreatureUnit unit)
	{
		if (unit == null || unit.Species == null)
		{
			return "-----";
		}

		try
		{
			CreatureForm form = unit.GetForm();
			if (form != null && !string.IsNullOrWhiteSpace(form.DisplayName))
			{
				return form.DisplayName;
			}
		}
		catch
		{
		}

		return unit.Species != null ? unit.Species.name : "-----";
	}
}
#endif
