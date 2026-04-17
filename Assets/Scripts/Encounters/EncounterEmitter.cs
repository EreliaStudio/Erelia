using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class EncounterEmitter : MonoBehaviour
{
	private const int DefaultDetectionHeightInCells = 3;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField, Min(1)] private int detectionHeightInCells = DefaultDetectionHeightInCells;
	[SerializeField] private EncounterResolver encounterResolver = new EncounterResolver();

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[EncounterEmitter] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the EncounterEmitter component.", Logger.Severity.Critical, this);
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
		if (worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, worldPosition, out Vector3Int standingCell))
		{
			return;
		}

		if (!TryGetBiome(standingCell, out BiomeDefinition biome))
		{
			return;
		}

		if (!TryFindEncounterRuleTagInActorColumn(standingCell, biome, out string encounterRuleTag, out Vector3Int taggedWorldCell))
		{
			return;
		}

		if (!encounterResolver.TryResolveEncounter(biome, encounterRuleTag, taggedWorldCell, out EncounterUnit[] selectedTeam, this))
		{
			return;
		}

		if (!biome.TryPickBoardConfiguration(out BoardConfiguration boardConfiguration))
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
			return;
		}

		BattleSetup battleSetup = new BattleSetup(
			selectedTeam,
			boardBuildResult.Board,
			worldPosition);

		EventCenter.EmitBattleStartRequested(battleSetup);
	}

	private bool TryGetBiome(Vector3Int standingCell, out BiomeDefinition biome)
	{
		biome = null;

		if (worldPresenter.MetaWorldData == null)
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

		return false;
	}

	private bool TryGetVoxelDefinition(Vector3Int worldCell, out VoxelDefinition voxelDefinition)
	{
		voxelDefinition = null;

		if (worldPresenter.WorldData == null ||
			worldPresenter.VoxelRegistry == null ||
			!worldPresenter.WorldData.TryGetCell(worldCell, out VoxelCell cell) ||
			cell == null ||
			cell.IsEmpty)
		{
			return false;
		}

		return worldPresenter.VoxelRegistry.TryGetVoxel(cell.Id, out voxelDefinition) && voxelDefinition != null;
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