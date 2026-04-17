using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class BattlePlayerController : MonoBehaviour
{
	[SerializeField] private GameObject cameraPrefab;
	[SerializeField] private Vector3 cameraLocalOffset = new Vector3(8f, 11f, 8f);
	[SerializeField, Min(0.1f)] private float panSpeed = 8f;
	[SerializeField] private InputActionReference panAction;
	[SerializeField] private InputActionReference orbitLeftAction;
	[SerializeField] private InputActionReference orbitRightAction;
	[SerializeField, Min(0f)] private float orbitSpeed = 120f;

	private GameObject cameraHolder;
	private GameObject spawnedCamera;
	private OrbitingObject orbitingObject;

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

	public void Bind(Vector3Int anchor, Vector3Int size)
	{
		boardAnchor = anchor;
		boardSize = size;

		Vector3 center = new Vector3(
			anchor.x + size.x * 0.5f,
			anchor.y,
			anchor.z + size.z * 0.5f);

		cameraHolder = new GameObject("BattleCameraHolder");
		cameraHolder.transform.position = center;

		Vector3 cameraWorldPosition = cameraHolder.transform.TransformPoint(cameraLocalOffset);
		spawnedCamera = Instantiate(cameraPrefab, cameraWorldPosition, Quaternion.identity, cameraHolder.transform);
		orbitingObject = spawnedCamera.GetComponent<OrbitingObject>();
	}

	public void Unbind()
	{
		if (cameraHolder != null)
		{
			Destroy(cameraHolder);
			cameraHolder = null;
		}

		spawnedCamera = null;
		orbitingObject = null;
	}

	private void Update()
	{
		if (cameraHolder == null)
		{
			return;
		}

		HandlePan();
		HandleOrbit();
	}

	private void HandlePan()
	{
		if (resolvedPanAction == null)
		{
			return;
		}

		Vector2 input = resolvedPanAction.ReadValue<Vector2>();
		if (input.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Vector3 delta = new Vector3(input.x, 0f, input.y) * (panSpeed * Time.deltaTime);
		Vector3 newPosition = cameraHolder.transform.position + delta;

		newPosition.x = Mathf.Clamp(newPosition.x, boardAnchor.x, boardAnchor.x + boardSize.x - 1);
		newPosition.z = Mathf.Clamp(newPosition.z, boardAnchor.z, boardAnchor.z + boardSize.z - 1);
		newPosition.y = cameraHolder.transform.position.y;

		cameraHolder.transform.position = newPosition;
	}

	private void HandleOrbit()
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
			orbitingObject.Orbit(axis * orbitSpeed, Time.deltaTime);
		}
	}

	private void ResolveActions()
	{
		resolvedPanAction = panAction != null ? panAction.action : null;
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
