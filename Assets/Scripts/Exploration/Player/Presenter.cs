using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Exploration.Player
{
	/// <summary>
	/// Presenter controlling exploration player movement.
	/// Resolves move input, converts it using camera orientation, and moves the player each frame.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View component associated with the player.
		/// </summary>
		[SerializeField] private Erelia.Exploration.Player.View view;

		/// <summary>
		/// Input action reference used for movement.
		/// </summary>
		[SerializeField] private InputActionReference moveAction;

		/// <summary>
		/// Movement speed in world units per second.
		/// </summary>
		[SerializeField] private float moveSpeed = 5f;

		/// <summary>
		/// Resolved input action used at runtime.
		/// This mean that we will use the moveAction and try to get its "real" action at runtime.
		/// </summary>
		private InputAction resolvedMoveAction;

		/// <summary>
		/// Backing model instance.
		/// </summary>
		private Erelia.Exploration.Player.Model model;

		/// <summary>
		/// Gets the current player model.
		/// </summary>
		public Erelia.Exploration.Player.Model Model => model;

		/// <summary>
		/// Assigns the player model.
		/// </summary>
		/// <param name="newModel">Model to assign.</param>
		public void SetModel(Erelia.Exploration.Player.Model newModel)
		{
			// Validate input to avoid inconsistent state.
			if (newModel == null)
			{
				throw new System.ArgumentNullException(nameof(newModel), "[Erelia.Exploration.Player.Presenter] Model cannot be null.");
			}

			// Store the model reference.
			model = newModel;
		}

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Ensure required references are assigned.
			if (view == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Presenter] View is not assigned.");
			}

			// Resolve input actions once at startup.
			ResolveActions();
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Enable input action so it starts producing values.
			resolvedMoveAction.Enable();
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Disable input action to avoid extra updates.
			resolvedMoveAction.Disable();
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Apply player movement each frame.
			ApplyMovement();
		}

		/// <summary>
		/// Applies movement based on input and camera orientation.
		/// </summary>
		private void ApplyMovement()
		{
			// Read input axis as a 2D vector.
			Vector2 moveInput = resolvedMoveAction.ReadValue<Vector2>();
			if (moveInput.sqrMagnitude < 0.0001f)
			{
				return;
			}

			// Use camera forward for movement direction on the XZ plane.
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

			// Compute right vector on the XZ plane.
			Vector3 right = Vector3.Cross(Vector3.up, forward);
			if (right.sqrMagnitude > 0.0001f)
			{
				right.Normalize();
			}
			else
			{
				right = Vector3.right;
			}

			// Combine input axes into world-space direction.
			Vector3 input = forward * moveInput.y + right * moveInput.x;
			if (input.sqrMagnitude > 1f)
			{
				input.Normalize();
			}

			// Move the player transform.
			view.gameObject.transform.position += input * moveSpeed * Time.deltaTime;
		}

		/// <summary>
		/// Resolves input actions from their references.
		/// </summary>
		private void ResolveActions()
		{
			// Validate input action reference.
			if (moveAction == null || moveAction.action == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Presenter] Move action is not assigned.");
			}

			// Store the resolved action.
			resolvedMoveAction = moveAction.action;
		}
	}
}
