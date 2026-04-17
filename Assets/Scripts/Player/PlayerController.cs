using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
	private const string DefaultInputAssetResourcePath = "Input/ExplorationPlayer";
	private const string ValidateActionName = "Player/Validate";
	private const string CancelActionName = "Player/Cancel";

	[SerializeField] private ActorPresenter controlledActor;
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private Camera inputCamera;
	[SerializeField] private InputActionAsset inputActionsAsset;
	[SerializeField] private InputActionReference validateAction;
	[SerializeField] private InputActionReference cancelAction;
	[SerializeField, Min(0.1f)] private float selectionMaxDistance = 128f;
	[SerializeField, Min(0.01f)] private float selectionStepDistance = 0.1f;

	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();
	private InputAction resolvedValidateAction;
	private InputAction resolvedCancelAction;
	private Vector3 lastCameraPosition;
	private Quaternion lastCameraRotation;
	private bool hasLastCameraTransform;
	private Vector2 lastPointerPosition;
	private bool hasLastPointerPosition;
	private bool selectionDirty = true;
	private Vector3Int? selectedVoxel;
	private Vector3Int? previousOverlaySelection;

	public Vector3Int? SelectedVoxel => selectedVoxel;

	private void Awake()
	{
		if (controlledActor == null)
		{
			Logger.LogError("[PlayerController] ControlledActor is not assigned in the inspector. Please assign an ActorPresenter to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (inputCamera == null)
		{
			Logger.LogError("[PlayerController] InputCamera is not assigned in the inspector. Please assign a Camera to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (worldPresenter == null)
		{
			Logger.LogError("[PlayerController] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the PlayerController component.", Logger.Severity.Critical, this);
		}

		if (inputActionsAsset == null)
		{
			inputActionsAsset = Resources.Load<InputActionAsset>(DefaultInputAssetResourcePath);
		}

		ResolveActions();
	}

	private void OnEnable()
	{
		EnableAction(resolvedValidateAction);
		EnableAction(resolvedCancelAction);
	}

	private void Update()
	{
		if (controlledActor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		UpdateSelectedVoxel();
		HandleCancelRequest();
		HandleMoveRequest();
		UpdateExplorationOverlay();
	}

	private void OnDisable()
	{
		DisableAction(resolvedValidateAction);
		DisableAction(resolvedCancelAction);
		ClearExplorationOverlay();
		selectedVoxel = null;
		previousOverlaySelection = null;
		hasLastCameraTransform = false;
		hasLastPointerPosition = false;
		selectionDirty = true;
		graphCache.Clear();
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
		if (worldPresenter == null || worldPresenter.WorldData == null || !HasOverlayStateChanged())
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
		if (worldPresenter == null || worldPresenter.WorldData == null)
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
		resolvedValidateAction = validateAction != null ? validateAction.action : FindAction(ValidateActionName);
		resolvedCancelAction = cancelAction != null ? cancelAction.action : FindAction(CancelActionName);
	}

	private InputAction FindAction(string actionName)
	{
		if (inputActionsAsset == null)
		{
			return null;
		}

		return inputActionsAsset.FindAction(actionName);
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
