using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerPresenter))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerPresenter playerPresenter;
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private Camera inputCamera;
	[SerializeField, Min(0.1f)] private float selectionMaxDistance = 128f;
	[SerializeField, Min(0.01f)] private float selectionStepDistance = 0.1f;
	[SerializeField, Min(0.01f)] private float movementSpeed = 4f;
	[SerializeField] private bool drawDebugGizmos = true;

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();
	private readonly List<Vector3Int> currentPath = new List<Vector3Int>();
	private Vector3Int? selectedVoxel;
	private int currentPathIndex;

	public Vector3Int? SelectedVoxel => selectedVoxel;
	public IReadOnlyList<Vector3Int> CurrentPath => currentPath;

	private void Reset()
	{
		if (playerPresenter == null)
		{
			playerPresenter = GetComponent<PlayerPresenter>();
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
		if (playerPresenter == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		UpdateSelectedVoxel();
		HandlePathRequest();
		UpdateMovement();
	}

	private void OnDisable()
	{
		currentPath.Clear();
		currentPathIndex = 0;
		selectedVoxel = null;
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
		if (!WorldVoxelRaycaster.TryRaycast(worldPresenter.WorldData, ray, selectionMaxDistance, selectionStepDistance, out WorldVoxelRaycaster.Hit hit))
		{
			return;
		}

		if (WorldPathfinder.TryResolveSelectableTarget(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, hit.WorldPosition, out Vector3Int targetWorldPosition))
		{
			selectedVoxel = targetWorldPosition;
		}
	}

	private void HandlePathRequest()
	{
		if (!selectedVoxel.HasValue || !WasMovementRequested())
		{
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, transform.position, out Vector3Int startWorldPosition))
		{
			return;
		}

		if (!WorldPathfinder.TryFindPath(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, startWorldPosition, selectedVoxel.Value, out List<Vector3Int> path))
		{
			return;
		}

		currentPath.Clear();
		currentPath.AddRange(path);
		currentPathIndex = currentPath.Count > 1 ? 1 : 0;
	}

	private void UpdateMovement()
	{
		if (currentPathIndex <= 0 || currentPathIndex >= currentPath.Count)
		{
			return;
		}

		Vector3Int targetCell = currentPath[currentPathIndex];
		if (!WorldPathfinder.TryGetStandingWorldPoint(worldPresenter.WorldData, worldPresenter.VoxelRegistry, targetCell, out Vector3 targetWorldPoint))
		{
			currentPath.Clear();
			currentPathIndex = 0;
			return;
		}

		transform.position = Vector3.MoveTowards(transform.position, targetWorldPoint, movementSpeed * Time.deltaTime);
		if ((transform.position - targetWorldPoint).sqrMagnitude > 0.0001f)
		{
			return;
		}

		transform.position = targetWorldPoint;
		currentPathIndex++;
		if (currentPathIndex >= currentPath.Count)
		{
			currentPath.Clear();
			currentPathIndex = 0;
		}
	}

	private void OnDrawGizmos()
	{
		if (!drawDebugGizmos)
		{
			return;
		}

		if (selectedVoxel.HasValue)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(new Vector3(selectedVoxel.Value.x + 0.5f, selectedVoxel.Value.y + 0.5f, selectedVoxel.Value.z + 0.5f), Vector3.one * 1.02f);
		}

		if (currentPath.Count <= 0 || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		Gizmos.color = Color.cyan;
		for (int i = 0; i < currentPath.Count; i++)
		{
			if (!WorldPathfinder.TryGetStandingWorldPoint(worldPresenter.WorldData, worldPresenter.VoxelRegistry, currentPath[i], out Vector3 point))
			{
				continue;
			}

			Gizmos.DrawSphere(point, 0.1f);
			if (i > 0 && WorldPathfinder.TryGetStandingWorldPoint(worldPresenter.WorldData, worldPresenter.VoxelRegistry, currentPath[i - 1], out Vector3 previousPoint))
			{
				Gizmos.DrawLine(previousPoint, point);
			}
		}
	}

	private static bool WasMovementRequested()
	{
		return Mouse.current != null ? Mouse.current.rightButton.wasPressedThisFrame : Input.GetMouseButtonDown(1);
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
