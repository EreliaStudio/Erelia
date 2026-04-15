using UnityEngine;

[DisallowMultipleComponent]
public class EncounterEmitter : MonoBehaviour
{
	private const int DefaultDetectionHeightInCells = 3;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField, Min(1)] private int detectionHeightInCells = DefaultDetectionHeightInCells;
	[SerializeField] private EncounterResolver encounterResolver = new EncounterResolver();

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

		if (!encounterResolver.TryResolveEncounter(biome, encounterRuleTag, taggedWorldCell, out BattleSetup battleSetup))
		{
			return;
		}

		Debug.Log($"EncounterEmitter: battle start requested from rule '{encounterRuleTag}' at {taggedWorldCell}.", this);
		EventCenter.EmitBattleStartRequested(battleSetup);
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
		string triggerTag = BiomeDefinition.NormalizeTriggerTag(GameRule.EncounterTriggerTag);

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
				string candidateTag = BiomeDefinition.NormalizeTriggerTag(voxelDefinition.Data.Tags[tagIndex]);
				if (string.Equals(candidateTag, triggerTag, System.StringComparison.Ordinal))
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
				string candidateTag = BiomeDefinition.NormalizeTriggerTag(voxelDefinition.Data.Tags[tagIndex]);
				if (string.IsNullOrEmpty(candidateTag) ||
				    string.Equals(candidateTag, triggerTag, System.StringComparison.Ordinal) ||
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
}
