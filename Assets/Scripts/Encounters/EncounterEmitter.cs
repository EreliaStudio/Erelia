using UnityEngine;

[DisallowMultipleComponent]
public class EncounterEmitter : MonoBehaviour
{
	private const string DefaultBushTag = "Bush";
	private const int DefaultDetectionHeightInCells = 3;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private string bushTag = DefaultBushTag;
	[SerializeField, Min(1)] private int detectionHeightInCells = DefaultDetectionHeightInCells;

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

		if (TryFindTaggedVoxelInActorColumn(standingCell, out Vector3Int taggedWorldCell))
		{
			Debug.Log($"EncounterEmitter: player is on a bush at {taggedWorldCell}.", this);
		}
	}

	private bool TryFindTaggedVoxelInActorColumn(Vector3Int standingCell, out Vector3Int taggedWorldCell)
	{
		taggedWorldCell = default;

		int maxHeight = Mathf.Max(1, detectionHeightInCells);
		for (int verticalOffset = 0; verticalOffset < maxHeight; verticalOffset++)
		{
			Vector3Int candidate = new Vector3Int(standingCell.x, standingCell.y + verticalOffset, standingCell.z);
			if (!TryGetVoxelDefinition(candidate, out VoxelDefinition voxelDefinition) || !voxelDefinition.HasTag(bushTag))
			{
				continue;
			}

			taggedWorldCell = candidate;
			return true;
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
