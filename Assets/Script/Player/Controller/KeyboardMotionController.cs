using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Controller
{
	[RequireComponent(typeof(PlayerInput))]
	public class KeyboardMotionController : MonoBehaviour
	{
		[SerializeField] private Transform cameraPivot;
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float rotateSpeed = 90f;
		[SerializeField] private string moveActionName = "Move";
		[SerializeField] private string rotateActionName = "RotatePlayer";

		private PlayerInput playerInput;
		private InputAction moveAction;
		private InputAction rotateAction;

		private void Awake()
		{
			playerInput = GetComponent<PlayerInput>();
			ResolveActions();
		}

		public void Configure(Transform pivot, float speed)
		{
			if (pivot != null)
			{
				cameraPivot = pivot;
			}

			if (speed > 0f)
			{
				moveSpeed = speed;
			}
		}

		private void OnEnable()
		{
			moveAction?.Enable();
			rotateAction?.Enable();
		}

		private void OnDisable()
		{
			moveAction?.Disable();
			rotateAction?.Disable();
		}

		private void Update()
		{
			ApplyMovement();
			ApplyRotation();
		}

		private void ApplyMovement()
		{
			if (moveAction == null)
			{
				return;
			}

			Vector3 forward = ResolveForward();
			Vector3 right = new Vector3(forward.z, 0f, -forward.x);
			Vector2 moveInput = moveAction.ReadValue<Vector2>();
			Vector3 input = forward * moveInput.y + right * moveInput.x;

			if (input.sqrMagnitude > 1f)
			{
				input.Normalize();
			}

			transform.position += input * moveSpeed * Time.deltaTime;
		}

		private Vector3 ResolveForward()
		{
			Transform pivot = cameraPivot != null ? cameraPivot : transform;
			Vector3 forward = pivot.forward;
			forward.y = 0f;
			return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
		}

		private void ApplyRotation()
		{
			if (rotateAction == null)
			{
				return;
			}

			float rotateInput = rotateAction.ReadValue<float>();
			if (Mathf.Abs(rotateInput) <= 0.01f)
			{
				return;
			}

			transform.Rotate(Vector3.up, rotateInput * rotateSpeed * Time.deltaTime, Space.World);
		}

		private void ResolveActions()
		{
			if (playerInput == null || playerInput.actions == null)
			{
				moveAction = null;
				rotateAction = null;
				return;
			}

			moveAction = playerInput.actions.FindAction($"Player/{moveActionName}", false)
				?? playerInput.actions.FindAction(moveActionName, false);

			InputAction actionFromMap = playerInput.currentActionMap != null
				? playerInput.currentActionMap.FindAction(rotateActionName, false)
				: null;

			rotateAction = actionFromMap
				?? playerInput.actions.FindAction($"Player/{rotateActionName}", false)
				?? playerInput.actions.FindAction(rotateActionName, false);
		}

	}
}
