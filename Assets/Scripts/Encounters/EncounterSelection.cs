using UnityEngine;

public sealed class EncounterSelection
{
	public readonly BiomeDefinition Biome;
	public readonly string TriggerTag;
	public readonly BiomeEncounterRule Rule;
	public readonly EncounterTier.Entry Entry;
	public readonly Vector3Int TriggerCell;
	public readonly ChunkCoordinates ChunkCoordinates;

	public EncounterSelection(
		BiomeDefinition biome,
		string triggerTag,
		BiomeEncounterRule rule,
		EncounterTier.Entry entry,
		Vector3Int triggerCell,
		ChunkCoordinates chunkCoordinates)
	{
		Biome = biome;
		TriggerTag = triggerTag;
		Rule = rule;
		Entry = entry;
		TriggerCell = triggerCell;
		ChunkCoordinates = chunkCoordinates;
	}
}
