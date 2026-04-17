using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBiomeDefinition", menuName = "Game/Biome Definition")]
public class BiomeDefinition : ScriptableObject
{
	public List<BoardConfiguration> BoardConfigurations = new List<BoardConfiguration>();

	[SerializedDictionary("Trigger Tag", "Encounter Rule")]
	public SerializedDictionary<string, BiomeEncounterRule> WildEncounterRulesByTriggerTag =
		new SerializedDictionary<string, BiomeEncounterRule>();

	public bool TryPickBoardConfiguration(out BoardConfiguration boardConfiguration, System.Random random = null)
	{
		boardConfiguration = null;

		if (BoardConfigurations == null || BoardConfigurations.Count == 0)
		{
			return false;
		}

		List<BoardConfiguration> validConfigurations = new List<BoardConfiguration>();
		for (int index = 0; index < BoardConfigurations.Count; index++)
		{
			BoardConfiguration candidate = BoardConfigurations[index];
			if (candidate != null)
			{
				validConfigurations.Add(candidate);
			}
		}

		if (validConfigurations.Count == 0)
		{
			return false;
		}

		int selectedIndex = (random != null) ?
			random.Next(0, validConfigurations.Count) :
			UnityEngine.Random.Range(0, validConfigurations.Count);

		boardConfiguration = validConfigurations[selectedIndex];
		return true;
	}

	public bool TryGetEncounterRule(string triggerTag, out BiomeEncounterRule rule)
	{
		rule = null;
		if (string.IsNullOrWhiteSpace(triggerTag) || WildEncounterRulesByTriggerTag == null)
		{
			return false;
		}

		string trimmedTag = CleanTriggerTag(triggerTag);
		if (WildEncounterRulesByTriggerTag.TryGetValue(trimmedTag, out rule) && rule != null)
		{
			return true;
		}

		foreach (var entry in WildEncounterRulesByTriggerTag)
		{
			if (!AreTriggerTagsEquivalent(entry.Key, trimmedTag))
			{
				continue;
			}

			rule = entry.Value;
			return rule != null;
		}

		return false;
	}

	public static string CleanTriggerTag(string triggerTag)
	{
		return string.IsNullOrWhiteSpace(triggerTag) ? string.Empty : triggerTag.Trim();
	}

	public static bool AreTriggerTagsEquivalent(string left, string right)
	{
		if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
		{
			return false;
		}

		return string.Equals(CleanTriggerTag(left), CleanTriggerTag(right), StringComparison.OrdinalIgnoreCase);
	}

	public IReadOnlyList<string> GetEncounterRuleTags()
	{
		List<string> tags = new List<string>();
		if (WildEncounterRulesByTriggerTag == null)
		{
			return tags;
		}

		foreach (var entry in WildEncounterRulesByTriggerTag)
		{
			string tag = CleanTriggerTag(entry.Key);
			if (string.IsNullOrEmpty(tag) || ContainsEquivalentTag(tags, tag))
			{
				continue;
			}

			tags.Add(tag);
		}

		return tags;
	}

	private static bool ContainsEquivalentTag(IReadOnlyList<string> tags, string candidate)
	{
		for (int index = 0; index < tags.Count; index++)
		{
			if (AreTriggerTagsEquivalent(tags[index], candidate))
			{
				return true;
			}
		}

		return false;
	}
}
