using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Exploration.Player
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Exploration.Player.View view;

		[SerializeField] private InputActionReference moveAction;

		[SerializeField] private float moveSpeed = 5f;

		private InputAction resolvedMoveAction;

		private Erelia.Exploration.Player.ExplorationPlayerState player;

		public Erelia.Exploration.Player.ExplorationPlayerState Player => player;

		public void SetPlayer(Erelia.Exploration.Player.ExplorationPlayerState playerState)
		{
			if (playerState == null)
			{
				throw new System.ArgumentNullException(nameof(playerState), "[Erelia.Exploration.Player.Presenter] Player state cannot be null.");
			}

			player = playerState;

			if (player.HasWorldPosition)
			{
				view.transform.position = player.WorldPosition;
				return;
			}

			player.SetWorldPosition(view.transform.position);
		}

		private void Awake()
		{
			if (view == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Presenter] View is not assigned.");
			}

			ResolveActions();
		}

		private void OnEnable()
		{
			resolvedMoveAction.Enable();
		}

		private void OnDisable()
		{
			resolvedMoveAction.Disable();
		}

		private void Update()
		{
			ApplyMovement();
		}

		private void ApplyMovement()
		{
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
			player?.SetWorldPosition(view.gameObject.transform.position);
		}

		private void ResolveActions()
		{
			if (moveAction == null || moveAction.action == null)
			{
				throw new System.Exception("[Erelia.Exploration.Player.Presenter] Move action is not assigned.");
			}

			resolvedMoveAction = moveAction.action;
		}
	}
}
