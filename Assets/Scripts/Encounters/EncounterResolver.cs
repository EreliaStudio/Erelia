using System;
using System.Text;
using UnityEngine;

[Serializable]
public class EncounterResolver
{
	[SerializeField] private int seed;
	[SerializeField, Min(0)] private int minimumStepsBetweenEncounters = 2;

	[NonSerialized] private System.Random random;
	[NonSerialized] private int stepsSinceLastEncounter;

	public bool TryResolveEncounter(
		BiomeDefinition biome,
		string triggerTag,
		Vector3Int triggerCell,
		out BattleSetup selection,
		bool debugLogging = false,
		UnityEngine.Object logContext = null)
	{
		selection = null;

		if (biome == null || !biome.TryGetEncounterRule(triggerTag, out BiomeEncounterRule rule) || rule == null)
		{
			LogDebug(debugLogging, logContext, $"No encounter rule was found for biome '{biome?.name ?? "<null>"}' and trigger '{triggerTag}' at {triggerCell}.");
			return false;
		}

		stepsSinceLastEncounter++;
		LogDebug(
			debugLogging,
			logContext,
			$"Encounter trigger matched. Biome='{biome.name}', Rule='{triggerTag}', Cell={triggerCell}, StepCounter={stepsSinceLastEncounter}/{minimumStepsBetweenEncounters}.");

		if (stepsSinceLastEncounter <= minimumStepsBetweenEncounters)
		{
			LogDebug(debugLogging, logContext, "Encounter blocked by minimum-steps cooldown.");
			return false;
		}

		float clampedChance = Mathf.Clamp01(rule.BaseChancePerStep);
		if (clampedChance <= 0f)
		{
			LogDebug(debugLogging, logContext, $"Encounter chance is {clampedChance:0.###}; no encounter can trigger.");
			return false;
		}

		EnsureRandom();
		double roll = random.NextDouble();
		LogDebug(debugLogging, logContext, $"Encounter roll: {roll:0.###} <= {clampedChance:0.###} ?");
		if (roll > clampedChance)
		{
			LogDebug(debugLogging, logContext, "Encounter chance did not match.");
			return false;
		}

		LogDebug(debugLogging, logContext, "Encounter chance matched.");

		if (!TrySelectWeightedEntry(rule.EncounterTable, out EncounterTier.Entry selectedEntry, debugLogging, logContext))
		{
			LogDebug(debugLogging, logContext, "No weighted team could be selected from the encounter table.");
			return false;
		}

		stepsSinceLastEncounter = 0;
		selection = BattleSetup.FromEntry(selectedEntry, null);
		LogDebug(
			debugLogging,
			logContext,
			$"Encounter team chosen: '{GetEntryLabel(selectedEntry)}'. Team={GetTeamSummary(selectedEntry)}");
		return true;
	}

	private bool TrySelectWeightedEntry(
		EncounterTable encounterTable,
		out EncounterTier.Entry selectedEntry,
		bool debugLogging,
		UnityEngine.Object logContext)
	{
		selectedEntry = null;

		if (encounterTable == null)
		{
			LogDebug(debugLogging, logContext, "Encounter table is null.");
			return false;
		}

		EncounterTier tier = encounterTable.GetTierForBadgeCount(0);
		if (tier == null || tier.WeightedTeams == null || tier.WeightedTeams.Count == 0)
		{
			LogDebug(debugLogging, logContext, "The selected encounter tier has no weighted teams.");
			return false;
		}

		int totalWeight = 0;
		for (int index = 0; index < tier.WeightedTeams.Count; index++)
		{
			EncounterTier.Entry candidate = tier.WeightedTeams[index];
			if (candidate == null || candidate.Weight <= 0)
			{
				continue;
			}

			totalWeight += candidate.Weight;
		}

		if (totalWeight <= 0)
		{
			LogDebug(debugLogging, logContext, "All weighted teams have a zero or invalid weight.");
			return false;
		}

		EnsureRandom();
		int selectedWeight = random.Next(totalWeight);
		LogDebug(debugLogging, logContext, $"Selecting weighted team with roll {selectedWeight} out of total weight {totalWeight}.");
		int cumulativeWeight = 0;

		for (int index = 0; index < tier.WeightedTeams.Count; index++)
		{
			EncounterTier.Entry candidate = tier.WeightedTeams[index];
			if (candidate == null || candidate.Weight <= 0)
			{
				continue;
			}

			cumulativeWeight += candidate.Weight;
			if (selectedWeight >= cumulativeWeight)
			{
				continue;
			}

			selectedEntry = candidate;
			LogDebug(
				debugLogging,
				logContext,
				$"Weighted team picked: '{GetEntryLabel(candidate)}' with weight {candidate.Weight} (cumulative {cumulativeWeight}).");
			return true;
		}

		return false;
	}

	private void EnsureRandom()
	{
		if (random == null)
		{
			random = new System.Random(seed);
		}
	}

	private static void LogDebug(bool enabled, UnityEngine.Object context, string message)
	{
		if (!enabled)
		{
			return;
		}

		Debug.Log($"[EncounterResolver] {message}", context);
	}

	private static string GetEntryLabel(EncounterTier.Entry entry)
	{
		if (entry == null)
		{
			return "<null>";
		}

		return string.IsNullOrWhiteSpace(entry.DisplayName) ? "<unnamed team>" : entry.DisplayName;
	}

	private static string GetTeamSummary(EncounterTier.Entry entry)
	{
		if (entry?.Team == null)
		{
			return "<none>";
		}

		StringBuilder builder = new StringBuilder();
		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			if (index > 0)
			{
				builder.Append(", ");
			}

			EncounterUnit unit = index < entry.Team.Length ? entry.Team[index] : null;
			builder.Append('[');
			builder.Append(index + 1);
			builder.Append("] ");
			builder.Append(GetUnitLabel(unit));
		}

		return builder.ToString();
	}

	private static string GetUnitLabel(EncounterUnit unit)
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

		return unit.Species.name;
	}
}
