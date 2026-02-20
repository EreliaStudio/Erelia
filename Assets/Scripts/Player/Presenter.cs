using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Player
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Player.View view;
		[SerializeField] private InputActionReference moveAction;
		[SerializeField] private float moveSpeed = 5f;

		private Erelia.Player.Model model;
		private InputAction resolvedMoveAction;

		private void Awake()
		{
			if (view == null)
			{
				Erelia.Logger.RaiseException("[Erelia.Player.Presenter] View is not assigned.");
			}

			model = new Erelia.Player.Model();
			ResolveActions();

			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(view.gameObject.transform.position);
			model.SetChunk(current - new World.Chunk.Coordinates(1, 1));
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
			UpdateChunk();
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
		}

		private void UpdateChunk()
		{
			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(view.gameObject.transform.position);

			if (model.SetChunk(current))
			{
				Erelia.Events.PlayerChunkChanged?.Invoke(current);
			}
		}

		private void ResolveActions()
		{
			if (moveAction == null || moveAction.action == null)
			{
				Erelia.Logger.RaiseException("[Erelia.Player.Presenter] Move action is not assigned.");
			}

			resolvedMoveAction = moveAction.action;
		}
	}
}
