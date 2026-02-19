using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Battle.Player.Controller
{
	[RequireComponent(typeof(PlayerInput))]
	public class KeyboardMotionController : MonoBehaviour
	{
		[SerializeField] private Transform cameraTransform;
		[SerializeField] private Transform boardTransform;
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

			Vector3 targetWorldPosition = transform.position + input * moveSpeed * Time.deltaTime;
			if (!TryGetBoardData(out Battle.Board.Model.Data boardData))
			{
				transform.position = targetWorldPosition;
				return;
			}

			Vector3 localCurrent = boardTransform != null
				? boardTransform.InverseTransformPoint(transform.position)
				: transform.position;
			Vector3 localTarget = boardTransform != null
				? boardTransform.InverseTransformPoint(targetWorldPosition)
				: targetWorldPosition;

			float clampedX = Mathf.Clamp(localTarget.x, 0f, boardData.SizeX - 1f);
			float clampedZ = Mathf.Clamp(localTarget.z, 0f, boardData.SizeZ - 1f);
			Vector3 localFinal = new Vector3(clampedX, localTarget.y, clampedZ);
			transform.position = boardTransform != null
				? boardTransform.TransformPoint(localFinal)
				: localFinal;
		}

		private bool TryGetBoardData(out Battle.Board.Model.Data data)
		{
			data = ServiceLocator.Instance?.BattleBoardService?.Data;
			if (data == null || data.Cells == null)
			{
				return false;
			}

			if (data.SizeX <= 0 || data.SizeY <= 0 || data.SizeZ <= 0)
			{
				return false;
			}

			return true;
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
