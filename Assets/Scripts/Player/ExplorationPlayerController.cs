using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ExplorationPlayerController : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private InputActionReference pointAction;
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
	private InputAction resolvedPointAction;
	private InputAction resolvedValidateAction;
	private InputAction resolvedCancelAction;
	private InputAction resolvedOrbitLeftAction;
	private InputAction resolvedOrbitRightAction;
	private Vector3 lastCameraPosition;
	private Quaternion lastCameraRotation;
	private bool hasLastCameraTransform;
	private Vector2 currentPointerPosition;
	private bool hasPointerPosition;
	private bool selectionDirty = true;
	private Vector3Int? selectedVoxel;
	private bool orbitLeftHeld;
	private bool orbitRightHeld;

	public Vector3Int? SelectedVoxel => selectedVoxel;

	public void Bind(ActorPresenter actor, Camera camera, OrbitingObject orbiting)
	{
		controlledActor = actor;
		inputCamera = camera;
		orbitingObject = orbiting;
		selectionDirty = true;
		if (hasPointerPosition)
		{
			RefreshSelectedVoxel(currentPointerPosition);
		}
	}

	public void Unbind()
	{
		controlledActor = null;
		inputCamera = null;
		orbitingObject = null;
		ClearSelectionMask();
		selectedVoxel = null;
		hasLastCameraTransform = false;
		hasPointerPosition = false;
		selectionDirty = true;
	}

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ExplorationPlayerController] WorldPresenter is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (pointAction == null)
		{
			Logger.LogError("[ExplorationPlayerController] PointAction is not assigned in the inspector.", Logger.Severity.Critical, this);
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
		SubscribeActionCallbacks();
		EnableAction(resolvedPointAction);
		EnableAction(resolvedValidateAction);
		EnableAction(resolvedCancelAction);
		EnableAction(resolvedOrbitLeftAction);
		EnableAction(resolvedOrbitRightAction);
	}

	private void OnDisable()
	{
		UnsubscribeActionCallbacks();
		DisableAction(resolvedPointAction);
		DisableAction(resolvedValidateAction);
		DisableAction(resolvedCancelAction);
		DisableAction(resolvedOrbitLeftAction);
		DisableAction(resolvedOrbitRightAction);
		ClearSelectionMask();
		selectedVoxel = null;
		hasLastCameraTransform = false;
		hasPointerPosition = false;
		selectionDirty = true;
		orbitLeftHeld = false;
		orbitRightHeld = false;
		graphCache.Clear();
	}

	private void Update()
	{
		HandleOrbitMotion();
	}

	private void HandleOrbitMotion()
	{
		if (controlledActor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null || orbitingObject == null)
		{
			return;
		}

		float axis = 0f;
		if (orbitLeftHeld)
		{
			axis -= 1f;
		}

		if (orbitRightHeld)
		{
			axis += 1f;
		}

		if (Mathf.Abs(axis) > 0.0001f)
		{
			orbitingObject.Orbit(axis, Time.deltaTime);
			selectionDirty = true;
			if (hasPointerPosition)
			{
				RefreshSelectedVoxel(currentPointerPosition);
			}
		}
	}

	private void RefreshSelectedVoxel(Vector2 pointerPosition)
	{
		if (controlledActor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		if (inputCamera == null)
		{
			SetSelectedVoxel(null);
			hasLastCameraTransform = false;
			hasPointerPosition = false;
			selectionDirty = true;
			return;
		}

		bool cameraChanged = !hasLastCameraTransform ||
		                     (inputCamera.transform.position - lastCameraPosition).sqrMagnitude > 0.000001f ||
		                     Quaternion.Angle(inputCamera.transform.rotation, lastCameraRotation) > 0.01f;

		if (!selectionDirty && !cameraChanged)
		{
			return;
		}

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

	private void OnPointPerformed(InputAction.CallbackContext context)
	{
		Vector2 pointerPosition = context.ReadValue<Vector2>();
		if (GameplayInputBlocker.IsPointerBlockedByUi(pointerPosition))
		{
			OnPointCanceled(context);
			return;
		}

		OnPointerMoved(pointerPosition);
	}

	private void OnPointCanceled(InputAction.CallbackContext _)
	{
		hasPointerPosition = false;
		if (controlledActor == null || worldPresenter == null || worldPresenter.WorldData == null)
		{
			return;
		}

		SetSelectedVoxel(null);
		hasLastCameraTransform = false;
		selectionDirty = true;
	}

	private void OnPointerMoved(Vector2 pointerPosition)
	{
		hasPointerPosition = true;
		currentPointerPosition = pointerPosition;
		selectionDirty = true;
		RefreshSelectedVoxel(pointerPosition);
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

	private void OnValidateRequested()
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction())
		{
			return;
		}

		if (controlledActor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null || !selectedVoxel.HasValue)
		{
			return;
		}

		EventCenter.EmitActorMoveRequested(new ActorMovementRequest(controlledActor, selectedVoxel.Value));
		SetSelectedVoxel(null);
		selectionDirty = true;
	}

	private void OnCancelRequested()
	{
		if (GameplayInputBlocker.ShouldBlockPointerAction())
		{
			return;
		}

		SetSelectedVoxel(null);
		selectionDirty = true;
	}

	private void OnValidateActionPerformed(InputAction.CallbackContext _)
	{
		OnValidateRequested();
	}

	private void OnCancelActionPerformed(InputAction.CallbackContext _)
	{
		OnCancelRequested();
	}

	private void OnOrbitLeftStarted(InputAction.CallbackContext _)
	{
		orbitLeftHeld = true;
	}

	private void OnOrbitLeftCanceled(InputAction.CallbackContext _)
	{
		orbitLeftHeld = false;
	}

	private void OnOrbitRightStarted(InputAction.CallbackContext _)
	{
		orbitRightHeld = true;
	}

	private void OnOrbitRightCanceled(InputAction.CallbackContext _)
	{
		orbitRightHeld = false;
	}

	private void ResolveActions()
	{
		resolvedPointAction = ResolvePointAction();
		resolvedValidateAction = validateAction != null ? validateAction.action : null;
		resolvedCancelAction = cancelAction != null ? cancelAction.action : null;
		resolvedOrbitLeftAction = orbitLeftAction != null ? orbitLeftAction.action : null;
		resolvedOrbitRightAction = orbitRightAction != null ? orbitRightAction.action : null;
	}

	private void SubscribeActionCallbacks()
	{
		if (resolvedPointAction != null)
		{
			resolvedPointAction.performed += OnPointPerformed;
			resolvedPointAction.canceled += OnPointCanceled;
		}

		if (resolvedValidateAction != null)
		{
			resolvedValidateAction.performed += OnValidateActionPerformed;
		}

		if (resolvedCancelAction != null)
		{
			resolvedCancelAction.performed += OnCancelActionPerformed;
		}

		if (resolvedOrbitLeftAction != null)
		{
			resolvedOrbitLeftAction.started += OnOrbitLeftStarted;
			resolvedOrbitLeftAction.canceled += OnOrbitLeftCanceled;
		}

		if (resolvedOrbitRightAction != null)
		{
			resolvedOrbitRightAction.started += OnOrbitRightStarted;
			resolvedOrbitRightAction.canceled += OnOrbitRightCanceled;
		}
	}

	private void UnsubscribeActionCallbacks()
	{
		if (resolvedPointAction != null)
		{
			resolvedPointAction.performed -= OnPointPerformed;
			resolvedPointAction.canceled -= OnPointCanceled;
		}

		if (resolvedValidateAction != null)
		{
			resolvedValidateAction.performed -= OnValidateActionPerformed;
		}

		if (resolvedCancelAction != null)
		{
			resolvedCancelAction.performed -= OnCancelActionPerformed;
		}

		if (resolvedOrbitLeftAction != null)
		{
			resolvedOrbitLeftAction.started -= OnOrbitLeftStarted;
			resolvedOrbitLeftAction.canceled -= OnOrbitLeftCanceled;
		}

		if (resolvedOrbitRightAction != null)
		{
			resolvedOrbitRightAction.started -= OnOrbitRightStarted;
			resolvedOrbitRightAction.canceled -= OnOrbitRightCanceled;
		}
	}

	private InputAction ResolvePointAction()
	{
		if (pointAction != null)
		{
			return pointAction.action;
		}

		if (validateAction != null && validateAction.action != null && validateAction.action.actionMap != null)
		{
			return validateAction.action.actionMap.FindAction("Point");
		}

		if (cancelAction != null && cancelAction.action != null && cancelAction.action.actionMap != null)
		{
			return cancelAction.action.actionMap.FindAction("Point");
		}

		return null;
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
