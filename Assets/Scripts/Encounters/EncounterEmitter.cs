using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class EncounterEmitter : MonoBehaviour
{
	private const int DefaultDetectionHeightInCells = 3;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField, Min(1)] private int detectionHeightInCells = DefaultDetectionHeightInCells;
	[SerializeField] private bool debugLogging = true;
	[SerializeField] private EncounterResolver encounterResolver = new EncounterResolver();
	[SerializeField] private BoardConfiguration boardConfiguration = new BoardConfiguration();

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();

	private void Reset()
	{
		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void Awake()
	{
		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void OnEnable()
	{
		EventCenter.PlayerMoved += OnPlayerMoved;
	}

	private void OnDisable()
	{
		EventCenter.PlayerMoved -= OnPlayerMoved;
		graphCache.Clear();
	}

	private void OnPlayerMoved(Vector3 worldPosition)
	{
		if (worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			LogDebug("World presenter, world data, or voxel registry is missing. Encounter evaluation skipped.");
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, worldPosition, out Vector3Int standingCell))
		{
			LogDebug($"Could not resolve a standing cell for player world position {worldPosition}.");
			return;
		}

		if (!TryGetBiome(standingCell, out BiomeDefinition biome))
		{
			LogDebug($"No biome was found for standing cell {standingCell}.");
			return;
		}

		if (!TryFindEncounterRuleTagInActorColumn(standingCell, biome, out string encounterRuleTag, out Vector3Int taggedWorldCell))
		{
			LogDebug($"No valid encounter trigger was found in the player column at standing cell {standingCell} for biome '{biome.name}'.");
			return;
		}

		LogDebug($"Player moved onto an encounter-trigger voxel at {taggedWorldCell}. Biome='{biome.name}', Rule='{encounterRuleTag}'.");

		if (!encounterResolver.TryResolveEncounter(biome, encounterRuleTag, taggedWorldCell, out BattleSetup battleSetup, debugLogging, this))
		{
			return;
		}

		BoardBuildResult boardBuildResult = BoardDataBuilder.Build(
			worldPresenter.WorldData,
			worldPresenter.VoxelRegistry,
			standingCell,
			boardConfiguration);

		if (boardBuildResult == null || boardBuildResult.Board == null)
		{
			LogDebug($"Board build failed for encounter at {taggedWorldCell}.");
			return;
		}

		worldPresenter.ShowBattleAreaBorder(boardBuildResult.BorderWorldCells);

		LogDebug(
			$"Battle board built successfully. Size={boardBuildResult.Board.Terrain.SizeX}x{boardBuildResult.Board.Terrain.SizeY}x{boardBuildResult.Board.Terrain.SizeZ}, BorderCellCount={boardBuildResult.BorderWorldCells?.Count ?? 0}.");
		LogDebug($"Battle start requested from rule '{encounterRuleTag}' at {taggedWorldCell}.");
		EventCenter.EmitBattleStartRequested(battleSetup.WithBoard(boardBuildResult.Board));
	}

	private bool TryGetBiome(Vector3Int standingCell, out BiomeDefinition biome)
	{
		biome = null;

		if (worldPresenter == null || worldPresenter.MetaWorldData == null)
		{
			return false;
		}

		if (!worldPresenter.MetaWorldData.TryGetChunkMeta(standingCell, out _, out ChunkMetaData chunkMetaData) || chunkMetaData == null)
		{
			return false;
		}

		biome = chunkMetaData.Biome;
		return biome != null;
	}

	private bool TryFindEncounterRuleTagInActorColumn(
		Vector3Int standingCell,
		BiomeDefinition biome,
		out string encounterRuleTag,
		out Vector3Int taggedWorldCell)
	{
		encounterRuleTag = string.Empty;
		taggedWorldCell = default;
		string triggerTag = GameRule.EncounterTriggerTag;
		bool foundEncounterTrigger = false;
		List<string> triggerVoxelTags = null;

		int maxHeight = Mathf.Max(1, detectionHeightInCells);
		for (int verticalOffset = 0; verticalOffset < maxHeight; verticalOffset++)
		{
			Vector3Int candidate = new Vector3Int(standingCell.x, standingCell.y + verticalOffset, standingCell.z);
			if (!TryGetVoxelDefinition(candidate, out VoxelDefinition voxelDefinition) || voxelDefinition?.Data?.Tags == null)
			{
				continue;
			}

			bool hasEncounterTrigger = false;
			for (int tagIndex = 0; tagIndex < voxelDefinition.Data.Tags.Count; tagIndex++)
			{
				string candidateTag = voxelDefinition.Data.Tags[tagIndex];
				if (BiomeDefinition.AreTriggerTagsEquivalent(candidateTag, triggerTag))
				{
					hasEncounterTrigger = true;
					break;
				}
			}

			if (!hasEncounterTrigger)
			{
				continue;
			}

			foundEncounterTrigger = true;
			triggerVoxelTags ??= CollectDistinctTags(voxelDefinition);

			for (int tagIndex = 0; tagIndex < voxelDefinition.Data.Tags.Count; tagIndex++)
			{
				string candidateTag = BiomeDefinition.CleanTriggerTag(voxelDefinition.Data.Tags[tagIndex]);
				if (string.IsNullOrEmpty(candidateTag) ||
				    BiomeDefinition.AreTriggerTagsEquivalent(candidateTag, triggerTag) ||
				    !biome.TryGetEncounterRule(candidateTag, out _))
				{
					continue;
				}

				encounterRuleTag = candidateTag;
				taggedWorldCell = candidate;
				return true;
			}
		}

		if (foundEncounterTrigger)
		{
			LogDebug(
				$"Encounter-trigger voxel found near {standingCell}, but none of its tags matched a biome rule in '{biome.name}'. " +
				$"VoxelTags=[{JoinTags(triggerVoxelTags)}], BiomeRuleTags=[{JoinTags(biome.GetEncounterRuleTags())}]");
		}

		return false;
	}

	private bool TryGetVoxelDefinition(Vector3Int worldCell, out VoxelDefinition voxelDefinition)
	{
		voxelDefinition = null;

		if (worldPresenter == null ||
			worldPresenter.WorldData == null ||
			worldPresenter.VoxelRegistry == null ||
			!worldPresenter.WorldData.TryGetCell(worldCell, out VoxelCell cell) ||
			cell == null ||
			cell.IsEmpty)
		{
			return false;
		}

		return worldPresenter.VoxelRegistry.TryGetVoxel(cell.Id, out voxelDefinition) && voxelDefinition != null;
	}

	private void LogDebug(string message)
	{
		if (!debugLogging)
		{
			return;
		}

		Debug.Log($"[EncounterEmitter] {message}", this);
	}

	private static List<string> CollectDistinctTags(VoxelDefinition voxelDefinition)
	{
		List<string> tags = new List<string>();
		if (voxelDefinition?.Data?.Tags == null)
		{
			return tags;
		}

		for (int index = 0; index < voxelDefinition.Data.Tags.Count; index++)
		{
			string tag = BiomeDefinition.CleanTriggerTag(voxelDefinition.Data.Tags[index]);
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
			if (BiomeDefinition.AreTriggerTagsEquivalent(tags[index], candidate))
			{
				return true;
			}
		}

		return false;
	}

	private static string JoinTags(IReadOnlyList<string> tags)
	{
		if (tags == null || tags.Count == 0)
		{
			return "<none>";
		}

		StringBuilder builder = new StringBuilder();
		for (int index = 0; index < tags.Count; index++)
		{
			if (index > 0)
			{
				builder.Append(", ");
			}

			builder.Append(tags[index]);
		}

		return builder.ToString();
	}
}
