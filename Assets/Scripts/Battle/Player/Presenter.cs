using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// Presenter controlling battle player movement.
	/// Reads move input each frame and moves the player relative to camera orientation.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View component used to orient movement.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.View view;
		/// <summary>
		/// Input action reference for movement.
		/// </summary>
		[SerializeField] private InputActionReference moveAction;
		/// <summary>
		/// Movement speed in world units per second.
		/// </summary>
		[SerializeField] private float moveSpeed = 5f;

		/// <summary>
		/// Resolved input action used at runtime.
		/// </summary>
		private InputAction resolvedMoveAction;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate references and resolve input actions.
			if (view == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Presenter] View is not assigned.");
			}

			ResolveActions();
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Enable the input action.
			resolvedMoveAction.Enable();
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Disable the input action.
			resolvedMoveAction.Disable();
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Apply movement each frame.
			ApplyMovement();
		}

		/// <summary>
		/// Applies movement based on input and camera orientation.
		/// </summary>
		private void ApplyMovement()
		{
			// Read input and convert it to world-space motion.
			Vector2 moveInput = resolvedMoveAction.ReadValue<Vector2>();
			if (moveInput.sqrMagnitude < 0.0001f)
			{
				return;
			}

			Transform pivot = view.LinkedCamera.transform;
			Vector3 forward = pivot.forward;
			forward.y = 0f;
			if (forward.sqrMagnitude > 0.0001f)
			{
				forward.Normalize();
			}
			else
			{
				forward = Vector3.forward;
			}

			Vector3 right = Vector3.Cross(Vector3.up, forward);
			if (right.sqrMagnitude > 0.0001f)
			{
				right.Normalize();
			}
			else
			{
				right = Vector3.right;
			}

			Vector3 input = forward * moveInput.y + right * moveInput.x;
			if (input.sqrMagnitude > 1f)
			{
				input.Normalize();
			}

			view.gameObject.transform.position += input * moveSpeed * Time.deltaTime;
		}

		/// <summary>
		/// Resolves input actions from their references.
		/// </summary>
		private void ResolveActions()
		{
			// Validate action reference and store the resolved action.
			if (moveAction == null || moveAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.Presenter] Move action is not assigned.");
			}

			resolvedMoveAction = moveAction.action;
		}
	}
}
