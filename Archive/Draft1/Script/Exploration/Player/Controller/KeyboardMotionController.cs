using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Exploration.Player.Controller
{
	[RequireComponent(typeof(PlayerInput))]
	public class KeyboardMotionController : MonoBehaviour
	{
		[SerializeField] private Transform cameraTransform;
		[SerializeField] private Transform bodyTransform;
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private string moveActionName = "Move";

		private PlayerInput playerInput;
		private InputAction moveAction;
		private void Awake()
		{
			playerInput = GetComponent<PlayerInput>();
			ResolveActions();
		}

		public void Configure(Transform pivot, float speed)
		{
			if (pivot != null)
			{
				cameraTransform = pivot;
			}

			if (speed > 0f)
			{
				moveSpeed = speed;
			}
		}

		private void OnEnable()
		{
			moveAction?.Enable();
		}

		private void OnDisable()
		{
			moveAction?.Disable();
		}

		private void Update()
		{
			ApplyMovement();
			ServiceLocator.Instance.PlayerService.UpdatePlayerPosition(transform.position);
		}

		private void ApplyMovement()
		{
			if (moveAction == null)
			{
				return;
			}

			Transform pivot = cameraTransform != null ? cameraTransform : transform;
			Vector3 forward = pivot.forward;
			Vector3 right = pivot.right;
			forward.y = 0f;
			right.y = 0f;
			if (forward.sqrMagnitude > 0.0001f)
			{
				forward.Normalize();
			}
			if (right.sqrMagnitude > 0.0001f)
			{
				right.Normalize();
			}
			Vector2 moveInput = moveAction.ReadValue<Vector2>();
			Vector3 input = forward * moveInput.y + right * moveInput.x;

			if (input.sqrMagnitude > 1f)
			{
				input.Normalize();
			}

			if (input.sqrMagnitude > 0.0001f && bodyTransform != null)
			{
				Quaternion targetRotation = Quaternion.LookRotation(input, Vector3.up);
				bodyTransform.rotation = targetRotation;
			}

			transform.position += input * moveSpeed * Time.deltaTime;
		}

		private void ResolveActions()
		{
			if (playerInput == null || playerInput.actions == null)
			{
				moveAction = null;
				return;
			}

			moveAction = playerInput.actions.FindAction($"Player/{moveActionName}", false)
				?? playerInput.actions.FindAction(moveActionName, false);
		}
	}
}
