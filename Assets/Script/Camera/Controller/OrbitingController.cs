using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera.Controller
{
	public class OrbitingController : MonoBehaviour
	{
		[SerializeField] private Vector3 localCameraPosition = new Vector3(-10f, 10f, -10f);
		[SerializeField] private Vector3 localLookAtPosition = Vector3.zero;
		[SerializeField] private float mouseOrbitSensitivity = 0.75f;
		[SerializeField] private float zoomSpeed = 0.2f;
		[SerializeField] private float minZoomMultiplier = 0.5f;
		[SerializeField] private float maxZoomMultiplier = 2.5f;
		[SerializeField] private string rotateActionName = "OrbitingCamera";

		private PlayerInput playerInput;
		private InputAction rotateAction;
		private float baseZoomDistance = 1f;

		private void Awake()
		{
			playerInput = GetComponentInParent<PlayerInput>();
			ResolveRotateAction();
		}

		private void Start()
		{
			if (localCameraPosition.sqrMagnitude > 0.0001f)
			{
				transform.localPosition = localCameraPosition;
			}

			baseZoomDistance = Mathf.Max(0.01f, transform.localPosition.magnitude);
			LookAtLocalPivot();
		}

		private void OnEnable()
		{
			rotateAction?.Enable();
		}

		private void OnDisable()
		{
			rotateAction?.Disable();
		}

		private void LateUpdate()
		{
			InputState state = ReadInputState();
			ApplyOrbitAndZoom(state);
			LookAtLocalPivot();
		}

		private InputState ReadInputState()
		{
			InputState state = new InputState();

			if (rotateAction == null)
			{
				return state;
			}

			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return state;
			}

			state.scroll = mouse.scroll.ReadValue().y;
			state.mouseOrbit = mouse.rightButton.isPressed ? rotateAction.ReadValue<float>() : 0f;

			return state;
		}

		private void ApplyOrbitAndZoom(InputState state)
		{
			if (Mathf.Abs(state.scroll) > 0.01f)
			{
				Vector3 current = transform.localPosition;
				float distance = current.magnitude;
				if (distance > 0.0001f)
				{
					float scrollSteps = state.scroll / 120f;
					float scale = 1f - scrollSteps * zoomSpeed;
					float minDistance = baseZoomDistance * minZoomMultiplier;
					float maxDistance = baseZoomDistance * maxZoomMultiplier;
					float newDistance = Mathf.Clamp(distance * scale, minDistance, maxDistance);
					transform.localPosition = current.normalized * newDistance;
				}
			}

			if (Mathf.Abs(state.mouseOrbit) > 0.01f)
			{
				TransformAroundLocalPivot(state.mouseOrbit * mouseOrbitSensitivity);
			}
		}

		private void TransformAroundLocalPivot(float angle)
		{
			Vector3 pivot = localLookAtPosition;
			Vector3 offset = transform.localPosition - pivot;
			Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
			transform.localPosition = pivot + rotation * offset;
		}

		private void LookAtLocalPivot()
		{
			Vector3 toPivot = localLookAtPosition - transform.localPosition;
			if (toPivot.sqrMagnitude <= 0.0001f)
			{
				return;
			}

			transform.localRotation = Quaternion.LookRotation(toPivot.normalized, Vector3.up);
		}

		private void ResolveRotateAction()
		{
			if (playerInput == null || playerInput.actions == null)
			{
				rotateAction = null;
				return;
			}

			InputAction actionFromMap = playerInput.currentActionMap != null
				? playerInput.currentActionMap.FindAction(rotateActionName, false)
				: null;

			rotateAction = actionFromMap
				?? playerInput.actions.FindAction($"Player/{rotateActionName}", false)
				?? playerInput.actions.FindAction(rotateActionName, false);
		}

		private struct InputState
		{
			public float scroll;
			public float mouseOrbit;
		}
	}
}
