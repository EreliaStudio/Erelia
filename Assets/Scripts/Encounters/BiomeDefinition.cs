using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBiomeDefinition", menuName = "Game/Biome Definition")]
public class BiomeDefinition : ScriptableObject
{
	[SerializedDictionary("Trigger Tag", "Encounter Rule")]
	public SerializedDictionary<string, BiomeEncounterRule> RulesByTriggerTag =
		new SerializedDictionary<string, BiomeEncounterRule>();

	public bool TryGetEncounterRule(string triggerTag, out BiomeEncounterRule rule)
	{
		rule = null;
		string normalizedTag = NormalizeTriggerTag(triggerTag);
		if (string.IsNullOrEmpty(normalizedTag) || RulesByTriggerTag == null)
		{
			return false;
		}

		if (RulesByTriggerTag.TryGetValue(normalizedTag, out rule) && rule != null)
		{
			return true;
		}

		foreach (var entry in RulesByTriggerTag)
		{
			if (!string.Equals(NormalizeTriggerTag(entry.Key), normalizedTag, StringComparison.Ordinal))
			{
				continue;
			}

			rule = entry.Value;
			return rule != null;
		}

		return false;
	}

	public static string NormalizeTriggerTag(string triggerTag)
	{
		return string.IsNullOrWhiteSpace(triggerTag) ? string.Empty : triggerTag.Trim().ToLowerInvariant();
	}
}
