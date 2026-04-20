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

		if (!biome.TryGetEncounterRule(encounterRuleTag, out BiomeEncounterRule encounterRule) || encounterRule == null)
		{
			return;
		}

		if (!encounterRule.TryPickBoardConfiguration(out BoardConfiguration boardConfiguration))
		{
			return;
		}

		BoardData boardData = BoardDataBuilder.Build(
			worldPresenter.WorldData,
			worldPresenter.VoxelRegistry,
			standingCell,
			boardConfiguration);

		if (boardData == null)
		{
			return;
		}

		BattleSetup battleSetup = new BattleSetup(
			selectedTeam,
			boardData,
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

}
