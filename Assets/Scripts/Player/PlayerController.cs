using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private InputActionReference validateAction;
	[SerializeField] private InputActionReference cancelAction;
	[SerializeField] private InputActionReference orbitLeftAction;
	[SerializeField] private InputActionReference orbitRightAction;
	[SerializeField, Min(0.1f)] private float selectionMaxDistance = 128f;
	[SerializeField, Min(0.01f)] private float selectionStepDistance = 0.1f;

	private ActorPresenter controlledActor;
	private Camera inputCamera;
	private OrbitingObject orbitingObject;

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();
	private InputAction resolvedValidateAction;
	private InputAction resolvedCancelAction;
	private InputAction resolvedOrbitLeftAction;
	private InputAction resolvedOrbitRightAction;
	private Vector3 lastCameraPosition;
	private Quaternion lastCameraRotation;
	private bool hasLastCameraTransform;
	private Vector2 lastPointerPosition;
	private bool hasLastPointerPosition;
	private bool selectionDirty = true;
	private Vector3Int? selectedVoxel;
	private Vector3Int? previousOverlaySelection;

	public Vector3Int? SelectedVoxel => selectedVoxel;

	public void Bind(ActorPresenter actor, Camera camera, OrbitingObject orbiting)
	{
		controlledActor = actor;
		inputCamera = camera;
		orbitingObject = orbiting;
		selectionDirty = true;
	}

	public void Unbind()
	{
		controlledActor = null;
		inputCamera = null;
		orbitingObject = null;
		selectedVoxel = null;
		previousOverlaySelection = null;
		selectionDirty = true;
	}

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[PlayerController] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (validateAction == null)
		{
			Logger.LogError("[PlayerController] ValidateAction is not assigned in the inspector. Please assign an InputActionReference to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (cancelAction == null)
		{
			Logger.LogError("[PlayerController] CancelAction is not assigned in the inspector. Please assign an InputActionReference to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (orbitLeftAction == null)
		{
			Logger.LogError("[PlayerController] OrbitLeftAction is not assigned in the inspector. Please assign an InputActionReference to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (orbitRightAction == null)
		{
			Logger.LogError("[PlayerController] OrbitRightAction is not assigned in the inspector. Please assign an InputActionReference to the PlayerController component.", Logger.Severity.Critical, this);
		}

		ResolveActions();
	}

	private void OnEnable()
	{
		EnableAction(resolvedValidateAction);
		EnableAction(resolvedCancelAction);
		EnableAction(resolvedOrbitLeftAction);
		EnableAction(resolvedOrbitRightAction);
	}

	private void OnDisable()
	{
		DisableAction(resolvedValidateAction);
		DisableAction(resolvedCancelAction);
		DisableAction(resolvedOrbitLeftAction);
		DisableAction(resolvedOrbitRightAction);
		ClearExplorationOverlay();
		selectedVoxel = null;
		previousOverlaySelection = null;
		hasLastCameraTransform = false;
		hasLastPointerPosition = false;
		selectionDirty = true;
		graphCache.Clear();
	}

	private void Update()
	{
		if (controlledActor == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		HandleOrbitRequest();
		UpdateSelectedVoxel();
		HandleCancelRequest();
		HandleMoveRequest();
		UpdateExplorationOverlay();
	}

	private void HandleOrbitRequest()
	{
		if (orbitingObject == null)
		{
			return;
		}

		float axis = 0f;
		if (resolvedOrbitLeftAction != null && resolvedOrbitLeftAction.IsPressed())
		{
			axis -= 1f;
		}

		if (resolvedOrbitRightAction != null && resolvedOrbitRightAction.IsPressed())
		{
			axis += 1f;
		}

		if (Mathf.Abs(axis) > 0.0001f)
		{
			orbitingObject.Orbit(axis, Time.deltaTime);
			selectionDirty = true;
		}
	}

	private void UpdateSelectedVoxel()
	{
		if (inputCamera == null || !TryGetPointerPosition(out Vector2 pointerPosition))
		{
			selectedVoxel = null;
			hasLastCameraTransform = false;
			hasLastPointerPosition = false;
			selectionDirty = true;
			return;
		}

		bool cameraChanged = !hasLastCameraTransform ||
		                     (inputCamera.transform.position - lastCameraPosition).sqrMagnitude > 0.000001f ||
		                     Quaternion.Angle(inputCamera.transform.rotation, lastCameraRotation) > 0.01f;
		bool pointerChanged = !hasLastPointerPosition || (pointerPosition - lastPointerPosition).sqrMagnitude > 0.0001f;

		if (!selectionDirty && !pointerChanged && !cameraChanged)
		{
			return;
		}

		lastPointerPosition = pointerPosition;
		hasLastPointerPosition = true;
		lastCameraPosition = inputCamera.transform.position;
		lastCameraRotation = inputCamera.transform.rotation;
		hasLastCameraTransform = true;
		selectionDirty = false;
		selectedVoxel = null;

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

	private void HandleMoveRequest()
	{
		if (!selectedVoxel.HasValue || !WasValidateRequested())
		{
			return;
		}

		EventCenter.EmitActorMoveRequested(new ActorMovementRequest(controlledActor, selectedVoxel.Value));
		selectedVoxel = null;
		selectionDirty = true;
	}

	private void HandleCancelRequest()
	{
		if (!WasCancelRequested())
		{
			return;
		}

		selectedVoxel = null;
		selectionDirty = true;
	}

	private void UpdateExplorationOverlay()
	{
		if (worldPresenter.WorldData == null || !HasOverlayStateChanged())
		{
			return;
		}

		ChunkCoordinates? previousCoordinates = previousOverlaySelection.HasValue
			? ChunkCoordinates.FromWorldVoxelPosition(previousOverlaySelection.Value)
			: (ChunkCoordinates?)null;
		ChunkCoordinates? currentCoordinates = selectedVoxel.HasValue
			? ChunkCoordinates.FromWorldVoxelPosition(selectedVoxel.Value)
			: (ChunkCoordinates?)null;

		if (previousCoordinates.HasValue)
		{
			worldPresenter.ClearChunkMasks(previousCoordinates.Value);
		}

		if (currentCoordinates.HasValue && (!previousCoordinates.HasValue || !currentCoordinates.Value.Equals(previousCoordinates.Value)))
		{
			worldPresenter.ClearChunkMasks(currentCoordinates.Value);
		}

		if (selectedVoxel.HasValue)
		{
			worldPresenter.TryAddMask(selectedVoxel.Value, VoxelMask.Selected);
		}

		if (previousCoordinates.HasValue)
		{
			worldPresenter.RebuildChunkOverlay(previousCoordinates.Value);
		}

		if (currentCoordinates.HasValue && (!previousCoordinates.HasValue || !currentCoordinates.Value.Equals(previousCoordinates.Value)))
		{
			worldPresenter.RebuildChunkOverlay(currentCoordinates.Value);
		}

		StoreOverlayState();
	}

	private void ClearExplorationOverlay()
	{
		if (worldPresenter.WorldData == null)
		{
			return;
		}

		worldPresenter.ClearAllChunkMasks();
		worldPresenter.RebuildAllChunkOverlays();
	}

	private bool WasValidateRequested()
	{
		if (resolvedValidateAction != null)
		{
			return resolvedValidateAction.WasPressedThisFrame();
		}

		return Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);
	}

	private bool WasCancelRequested()
	{
		if (resolvedCancelAction != null)
		{
			return resolvedCancelAction.WasPressedThisFrame();
		}

		return Mouse.current != null ? Mouse.current.rightButton.wasPressedThisFrame : Input.GetMouseButtonDown(1);
	}

	private bool HasOverlayStateChanged()
	{
		return previousOverlaySelection != selectedVoxel;
	}

	private void StoreOverlayState()
	{
		previousOverlaySelection = selectedVoxel;
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

	private void ResolveActions()
	{
		resolvedValidateAction = validateAction != null ? validateAction.action : null;
		resolvedCancelAction = cancelAction != null ? cancelAction.action : null;
		resolvedOrbitLeftAction = orbitLeftAction != null ? orbitLeftAction.action : null;
		resolvedOrbitRightAction = orbitRightAction != null ? orbitRightAction.action : null;
	}

	private static void EnableAction(InputAction action)
	{
		if (action != null && !action.enabled)
		{
			action.Enable();
		}
	}

	private static void DisableAction(InputAction action)
	{
		if (action != null && action.enabled)
		{
			action.Disable();
		}
	}
}
