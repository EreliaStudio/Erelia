using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class OrbitingObject : MonoBehaviour
{
	private const string DefaultInputAssetResourcePath = "Input/ExplorationPlayer";
	private const string OrbitLeftActionName = "Player/OrbitLeft";
	private const string OrbitRightActionName = "Player/OrbitRight";

	[SerializeField] private Transform orbitingTransform;
	[SerializeField] private Transform orbitSpace;
	[SerializeField] private Vector3 orbitTargetLocalPoint = Vector3.zero;
	[SerializeField] private InputActionAsset inputActionsAsset;
	[SerializeField] private InputActionReference orbitLeftAction;
	[SerializeField] private InputActionReference orbitRightAction;
	[SerializeField, Min(0f)] private float keyOrbitSpeed = 120f;

	private InputAction resolvedOrbitLeftAction;
	private InputAction resolvedOrbitRightAction;

	private void Reset()
	{
		if (orbitingTransform == null)
		{
			Camera childCamera = GetComponentInChildren<Camera>();
			if (childCamera != null)
			{
				orbitingTransform = childCamera.transform;
			}
		}

		ResolveOrbitSpace();

		if (inputActionsAsset == null)
		{
			inputActionsAsset = Resources.Load<InputActionAsset>(DefaultInputAssetResourcePath);
		}
	}

	private void Awake()
	{
		ResolveOrbitSpace();

		if (inputActionsAsset == null)
		{
			inputActionsAsset = Resources.Load<InputActionAsset>(DefaultInputAssetResourcePath);
		}

		ResolveActions();
		LookAtTarget();
	}

	private void Start()
	{
		LookAtTarget();
	}

	private void OnValidate()
	{
		if (orbitingTransform == null)
		{
			Camera childCamera = GetComponentInChildren<Camera>();
			if (childCamera != null)
			{
				orbitingTransform = childCamera.transform;
			}
		}

		ResolveOrbitSpace();

		if (!Application.isPlaying)
		{
			LookAtTarget();
		}
	}

	private void OnEnable()
	{
		EnableAction(resolvedOrbitLeftAction);
		EnableAction(resolvedOrbitRightAction);
	}

	private void OnDisable()
	{
		DisableAction(resolvedOrbitLeftAction);
		DisableAction(resolvedOrbitRightAction);
	}

	private void Update()
	{
		if (orbitingTransform == null)
		{
			return;
		}

		float orbitAxis = 0f;
		if (resolvedOrbitLeftAction != null && resolvedOrbitLeftAction.IsPressed())
		{
			orbitAxis -= 1f;
		}

		if (resolvedOrbitRightAction != null && resolvedOrbitRightAction.IsPressed())
		{
			orbitAxis += 1f;
		}

		if (Mathf.Abs(orbitAxis) <= 0.0001f)
		{
			return;
		}

		Transform activeOrbitSpace = GetOrbitSpace();
		Vector3 localOrbitingPosition = activeOrbitSpace.InverseTransformPoint(orbitingTransform.position);
		Vector3 localOffset = localOrbitingPosition - orbitTargetLocalPoint;
		if (localOffset.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Quaternion rotation = Quaternion.AngleAxis(orbitAxis * keyOrbitSpeed * Time.deltaTime, Vector3.up);
		Vector3 rotatedLocalPosition = orbitTargetLocalPoint + rotation * localOffset;
		orbitingTransform.position = activeOrbitSpace.TransformPoint(rotatedLocalPosition);
	}

	private void LateUpdate()
	{
		LookAtTarget();
	}

	[ContextMenu("Look At Target")]
	private void ForceLookAtTarget()
	{
		LookAtTarget();
	}

	private void ResolveActions()
	{
		resolvedOrbitLeftAction = orbitLeftAction != null ? orbitLeftAction.action : FindAction(OrbitLeftActionName);
		resolvedOrbitRightAction = orbitRightAction != null ? orbitRightAction.action : FindAction(OrbitRightActionName);
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

	private void LookAtTarget()
	{
		if (orbitingTransform == null)
		{
			return;
		}

		Transform activeOrbitSpace = GetOrbitSpace();
		Vector3 localOrbitingPosition = activeOrbitSpace.InverseTransformPoint(orbitingTransform.position);
		Vector3 localDirection = orbitTargetLocalPoint - localOrbitingPosition;
		if (localDirection.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Quaternion localRotation = Quaternion.LookRotation(localDirection.normalized, Vector3.up);
		orbitingTransform.rotation = activeOrbitSpace.rotation * localRotation;
	}

	private void ResolveOrbitSpace()
	{
		if (orbitSpace != null)
		{
			return;
		}

		if (orbitingTransform != null && orbitingTransform.parent != null)
		{
			orbitSpace = orbitingTransform.parent;
			return;
		}

		orbitSpace = transform;
	}

	private Transform GetOrbitSpace()
	{
		if (orbitSpace == null)
		{
			ResolveOrbitSpace();
		}

		return orbitSpace != null ? orbitSpace : transform;
	}
}
