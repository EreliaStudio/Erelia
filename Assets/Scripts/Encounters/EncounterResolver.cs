using System;
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
		out EncounterSelection selection)
	{
		selection = null;

		if (biome == null || !biome.TryGetEncounterRule(triggerTag, out BiomeEncounterRule rule) || rule == null)
		{
			return false;
		}

		stepsSinceLastEncounter++;
		if (stepsSinceLastEncounter <= minimumStepsBetweenEncounters)
		{
			return false;
		}

		float clampedChance = Mathf.Clamp01(rule.BaseChancePerStep);
		if (clampedChance <= 0f)
		{
			return false;
		}

		EnsureRandom();
		if (random.NextDouble() > clampedChance)
		{
			return false;
		}

		if (!TrySelectWeightedEntry(rule.EncounterTable, out EncounterTier.Entry selectedEntry))
		{
			return false;
		}

		stepsSinceLastEncounter = 0;
		ChunkCoordinates chunkCoordinates = ChunkCoordinates.FromWorldVoxelPosition(triggerCell);
		selection = new EncounterSelection(
			biome,
			BiomeDefinition.NormalizeTriggerTag(triggerTag),
			rule,
			selectedEntry,
			triggerCell,
			chunkCoordinates);
		return true;
	}

	private bool TrySelectWeightedEntry(EncounterTable encounterTable, out EncounterTier.Entry selectedEntry)
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
