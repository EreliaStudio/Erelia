using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ExplorationPlayerController : MonoBehaviour
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

	private readonly WorldTraversalGraphCache graphCache = new();
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
		ClearSelectionMask();
		selectionDirty = true;
	}

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ExplorationPlayerController] WorldPresenter is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (validateAction == null)
		{
			Logger.LogError("[ExplorationPlayerController] ValidateAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (cancelAction == null)
		{
			Logger.LogError("[ExplorationPlayerController] CancelAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (orbitLeftAction == null)
		{
			Logger.LogError("[ExplorationPlayerController] OrbitLeftAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (orbitRightAction == null)
		{
			Logger.LogError("[ExplorationPlayerController] OrbitRightAction is not assigned in the inspector.", Logger.Severity.Critical, this);
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
		ClearSelectionMask();
		selectedVoxel = null;
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
			SetSelectedVoxel(null);
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
			SetSelectedVoxel(null);
			return;
		}

		if (WorldPathfinder.TryResolveSelectableTarget(worldPresenter.WorldData, worldPresenter.VoxelRegistry, graphCache, hit.WorldPosition, out Vector3Int targetWorldPosition))
		{
			SetSelectedVoxel(targetWorldPosition);
		}
		else
		{
			SetSelectedVoxel(null);
		}
	}

	private void SetSelectedVoxel(Vector3Int? newVoxel)
	{
		if (newVoxel == selectedVoxel)
		{
			return;
		}

		if (selectedVoxel.HasValue && worldPresenter.WorldData != null)
		{
			worldPresenter.TryRemoveMask(selectedVoxel.Value, VoxelMask.Selected);
			worldPresenter.RebuildChunkOverlay(ChunkCoordinates.FromWorldVoxelPosition(selectedVoxel.Value));
		}

		selectedVoxel = newVoxel;

		if (selectedVoxel.HasValue && worldPresenter.WorldData != null)
		{
			worldPresenter.TryAddMask(selectedVoxel.Value, VoxelMask.Selected);
			worldPresenter.RebuildChunkOverlay(ChunkCoordinates.FromWorldVoxelPosition(selectedVoxel.Value));
		}
	}

	private void ClearSelectionMask()
	{
		if (selectedVoxel.HasValue && worldPresenter.WorldData != null)
		{
			worldPresenter.TryRemoveMask(selectedVoxel.Value, VoxelMask.Selected);
			worldPresenter.RebuildChunkOverlay(ChunkCoordinates.FromWorldVoxelPosition(selectedVoxel.Value));
		}
	}

	private void HandleMoveRequest()
	{
		if (!selectedVoxel.HasValue || !WasValidateRequested())
		{
			return;
		}

		EventCenter.EmitActorMoveRequested(new ActorMovementRequest(controlledActor, selectedVoxel.Value));
		SetSelectedVoxel(null);
		selectionDirty = true;
	}

	private void HandleCancelRequest()
	{
		if (!WasCancelRequested())
		{
			return;
		}

		SetSelectedVoxel(null);
		selectionDirty = true;
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
