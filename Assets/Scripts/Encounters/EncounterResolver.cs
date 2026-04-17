using System;
using System.Text;
using UnityEngine;

[Serializable]
public class EncounterResolver
{
	[SerializeField] private int seed;

	[NonSerialized] private System.Random random;
	[NonSerialized] private bool hasLastCheckedCell;
	[NonSerialized] private Vector3Int lastCheckedCell;

	public bool TryResolveEncounter(
		BiomeDefinition biome,
		string triggerTag,
		Vector3Int triggerCell,
		out EncounterUnit[] selectedTeam,
		UnityEngine.Object logContext = null)
	{
		selectedTeam = null;

		if (biome == null || !biome.TryGetEncounterRule(triggerTag, out BiomeEncounterRule rule) || rule == null)
		{
			return false;
		}

		if (hasLastCheckedCell && lastCheckedCell == triggerCell)
		{
			return false;
		}

		lastCheckedCell = triggerCell;
		hasLastCheckedCell = true;

		float clampedChance = Mathf.Clamp01(rule.BaseChancePerStep);
		if (clampedChance <= 0f)
		{
			return false;
		}

		EnsureRandom();
		double roll = random.NextDouble();

		if (roll > clampedChance)
		{
			return false;
		}

		if (!TrySelectWeightedEntry(rule.EncounterTable, out EncounterTier.Entry selectedEntry, logContext))
		{
			return false;
		}

		selectedTeam = selectedEntry.Team;

		return true;
	}

	private bool TrySelectWeightedEntry(
		EncounterTable encounterTable,
		out EncounterTier.Entry selectedEntry,
		UnityEngine.Object logContext)
	{
		selectedEntry = null;

		if (encounterTable == null)
		{
			return false;
		}

		EncounterTier tier = encounterTable.GetTierForBadgeCount(0);
		if (tier == null || tier.WeightedTeams == null || tier.WeightedTeams.Count == 0)
		{
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
			return false;
		}

		EnsureRandom();
		int selectedWeight = random.Next(totalWeight);
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
}