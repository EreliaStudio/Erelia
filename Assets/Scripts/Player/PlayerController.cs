using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(PlayerPresenter))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private Actor actor;
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private Camera inputCamera;
	[SerializeField, Min(0.1f)] private float selectionMaxDistance = 128f;
	[SerializeField, Min(0.01f)] private float selectionStepDistance = 0.1f;

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();
	private readonly List<Vector3Int> previewPath = new List<Vector3Int>();
	private readonly List<Vector3Int> previousPreviewPath = new List<Vector3Int>();
	private Vector3Int? selectedVoxel;
	private Vector3Int? previousOverlaySelection;

	public Vector3Int? SelectedVoxel => selectedVoxel;
	public IReadOnlyList<Vector3Int> CurrentPath => previewPath;

	private void Reset()
	{
		if (actor == null)
		{
			actor = GetComponent<Actor>();
		}

		if (inputCamera == null)
		{
			inputCamera = GetComponentInChildren<Camera>();
		}

		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void Awake()
	{
		if (actor == null)
		{
			actor = GetComponent<Actor>();
		}

		if (inputCamera == null)
		{
			inputCamera = GetComponentInChildren<Camera>();
		}

		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void Update()
	{
		if (actor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		UpdateSelectedVoxel();
		UpdatePreviewPath();
		UpdateExplorationOverlay();
		HandleMoveRequest();
	}

	private void OnDisable()
	{
		ClearExplorationOverlay();
		previewPath.Clear();
		previousPreviewPath.Clear();
		selectedVoxel = null;
		previousOverlaySelection = null;
		graphCache.Clear();
	}

	private void UpdateSelectedVoxel()
	{
		selectedVoxel = null;

		if (inputCamera == null || !TryGetPointerPosition(out Vector2 pointerPosition))
		{
			return;
		}

		Ray ray = inputCamera.ScreenPointToRay(pointerPosition);
		if (!WorldVoxelRaycaster.TryRaycast(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				ray,
				selectionMaxDistance,
				selectionStepDistance,
				WorldVoxelRaycaster.ByTraversal(VoxelTraversal.Obstacle),
				out WorldVoxelRaycaster.Hit hit))
		{
			return;
		}

		if (WorldPathfinder.TryResolveSelectableTarget(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, hit.WorldPosition, out Vector3Int targetWorldPosition))
		{
			selectedVoxel = targetWorldPosition;
		}
	}

	private void UpdatePreviewPath()
	{
		previewPath.Clear();

		if (!selectedVoxel.HasValue)
		{
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, transform.position, out Vector3Int startWorldPosition))
		{
			return;
		}

		if (WorldPathfinder.TryFindPath(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, startWorldPosition, selectedVoxel.Value, out List<Vector3Int> path))
		{
			previewPath.AddRange(path);
		}
	}

	private void HandleMoveRequest()
	{
		if (!selectedVoxel.HasValue || !WasMovementRequested())
		{
			return;
		}

		EventCenter.EmitActorMoveRequested(new ActorMovementRequest(actor, selectedVoxel.Value));
	}

	private void UpdateExplorationOverlay()
	{
		if (worldPresenter == null || worldPresenter.WorldData == null || !HasOverlayStateChanged())
		{
			return;
		}

		worldPresenter.ClearAllChunkMasks();

		for (int i = 1; i < previewPath.Count; i++)
		{
			worldPresenter.TryAddMask(previewPath[i], VoxelMask.MovementRange);
		}

		if (selectedVoxel.HasValue)
		{
			worldPresenter.TryAddMask(selectedVoxel.Value, VoxelMask.Selected);
		}

		worldPresenter.RebuildAllChunkOverlays();
		StoreOverlayState();
	}

	private void ClearExplorationOverlay()
	{
		if (worldPresenter == null || worldPresenter.WorldData == null)
		{
			return;
		}

		worldPresenter.ClearAllChunkMasks();
		worldPresenter.RebuildAllChunkOverlays();
	}

	private static bool WasMovementRequested()
	{
		return Mouse.current != null ? Mouse.current.rightButton.wasPressedThisFrame : Input.GetMouseButtonDown(1);
	}

	private bool HasOverlayStateChanged()
	{
		if (previousOverlaySelection != selectedVoxel)
		{
			return true;
		}

		if (previousPreviewPath.Count != previewPath.Count)
		{
			return true;
		}

		for (int i = 0; i < previewPath.Count; i++)
		{
			if (previousPreviewPath[i] != previewPath[i])
			{
				return true;
			}
		}

		return false;
	}

	private void StoreOverlayState()
	{
		previousOverlaySelection = selectedVoxel;
		previousPreviewPath.Clear();
		previousPreviewPath.AddRange(previewPath);
	}

	private static bool TryGetPointerPosition(out Vector2 pointerPosition)
	{
		if (Mouse.current != null)
		{
			pointerPosition = Mouse.current.position.ReadValue();
			return true;
		}

		pointerPosition = Input.mousePosition;
		return true;
	}
}
