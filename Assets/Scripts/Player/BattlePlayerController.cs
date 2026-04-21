using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class BattlePlayerController : MonoBehaviour
{
	[SerializeField] private GameObject cameraPrefab;
	[SerializeField] private Vector3 cameraLocalOffset = new Vector3(8f, 11f, 8f);
	[SerializeField, Min(0.1f)] private float panSpeed = 8f;
	[SerializeField, Min(1f)] private float orbitSpeed = 120f;
	[SerializeField] private InputActionReference panAction;
	[SerializeField] private InputActionReference orbitLeftAction;
	[SerializeField] private InputActionReference orbitRightAction;
	private GameObject cameraHolder;
	private GameObject spawnedCamera;

	private Vector3Int boardAnchor;
	private Vector3Int boardSize;

	private InputAction resolvedPanAction;
	private InputAction resolvedOrbitLeftAction;
	private InputAction resolvedOrbitRightAction;

	public Camera ActiveCamera => spawnedCamera != null ? spawnedCamera.GetComponentInChildren<Camera>() : null;

	private void Awake()
	{
		if (cameraPrefab == null)
		{
			Logger.LogError("[BattlePlayerController] CameraPrefab is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (panAction == null)
		{
			Logger.LogError("[BattlePlayerController] PanAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (orbitLeftAction == null)
		{
			Logger.LogError("[BattlePlayerController] OrbitLeftAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (orbitRightAction == null)
		{
			Logger.LogError("[BattlePlayerController] OrbitRightAction is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		ResolveActions();
	}

	private void OnEnable()
	{
		EnableAction(resolvedPanAction);
		EnableAction(resolvedOrbitLeftAction);
		EnableAction(resolvedOrbitRightAction);
	}

	private void OnDisable()
	{
		DisableAction(resolvedPanAction);
		DisableAction(resolvedOrbitLeftAction);
		DisableAction(resolvedOrbitRightAction);
	}

	public void Bind(Vector3Int p_boardAnchor, Vector3Int p_boardSize, Vector3 p_playerWorldPosition)
	{
		boardAnchor = p_boardAnchor;
		boardSize = p_boardSize;

		if (cameraHolder == null)
		{
			cameraHolder = new GameObject("BattleCameraHolder");

			Transform parentTransform = transform.parent != null ? transform.parent : transform;
			cameraHolder.transform.SetParent(parentTransform, true);
		}

		cameraHolder.transform.position = p_playerWorldPosition;
		cameraHolder.transform.rotation = Quaternion.identity;

		if (spawnedCamera == null && cameraPrefab != null)
		{
			spawnedCamera = Instantiate(cameraPrefab, cameraHolder.transform);
		}
		else if (spawnedCamera != null)
		{
			spawnedCamera.transform.SetParent(cameraHolder.transform, false);
		}

		RefreshCameraTransform();
	}

	public void Unbind()
	{
		if (cameraHolder != null)
		{
			Destroy(cameraHolder);
			cameraHolder = null;
		}

		spawnedCamera = null;
	}

	private void Update()
	{
		if (cameraHolder == null)
		{
			return;
		}

		HandlePan();
		HandleOrbit();
		RefreshCameraTransform();
	}

	private void HandlePan()
	{
		if (resolvedPanAction == null || cameraHolder == null)
		{
			return;
		}

		Vector2 input = resolvedPanAction.ReadValue<Vector2>();
		if (input.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Transform referenceTransform = ActiveCamera != null ? ActiveCamera.transform : cameraHolder.transform;

		Vector3 flatForward = referenceTransform.forward;
		Vector3 flatRight = referenceTransform.right;

		flatForward.y = 0f;
		flatRight.y = 0f;

		if (flatForward.sqrMagnitude <= 0.0001f || flatRight.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		flatForward.Normalize();
		flatRight.Normalize();

		Vector3 movement = (flatRight * input.x + flatForward * input.y) * (panSpeed * Time.deltaTime);
		Vector3 newPosition = cameraHolder.transform.position + movement;

		newPosition.x = Mathf.Clamp(newPosition.x, boardAnchor.x, boardAnchor.x + boardSize.x);
		newPosition.z = Mathf.Clamp(newPosition.z, boardAnchor.z, boardAnchor.z + boardSize.z);

		cameraHolder.transform.position = newPosition;
	}

	private void HandleOrbit()
	{
		if (cameraHolder == null)
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

		if (Mathf.Abs(axis) <= 0.0001f)
		{
			return;
		}

		cameraHolder.transform.Rotate(Vector3.up, axis * orbitSpeed * Time.deltaTime, Space.World);
	}

	private void RefreshCameraTransform()
	{
		if (cameraHolder == null || spawnedCamera == null)
		{
			return;
		}

		spawnedCamera.transform.localPosition = cameraLocalOffset;
		spawnedCamera.transform.LookAt(cameraHolder.transform.position, Vector3.up);
	}

	private void ResolveActions()
	{
		resolvedPanAction = panAction != null ? panAction.action : null;
		resolvedOrbitLeftAction = orbitLeftAction != null ? orbitLeftAction.action : null;
		resolvedOrbitRightAction = orbitRightAction != null ? orbitRightAction.action : null;
	}

	private static void EnableAction(InputAction p_action)
	{
		if (p_action != null && !p_action.enabled)
		{
			p_action.Enable();
		}
	}

	private static void DisableAction(InputAction p_action)
	{
		if (p_action != null && p_action.enabled)
		{
			p_action.Disable();
		}
	}
}
