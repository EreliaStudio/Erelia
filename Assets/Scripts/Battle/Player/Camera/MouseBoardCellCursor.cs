using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player.Camera
{
	/// <summary>
	/// Raycasts from the mouse cursor onto the battle board to resolve grid cells.
	/// Emits cell change and hover cleared events as the cursor moves.
	/// </summary>
	public sealed class MouseBoardCellCursor : MonoBehaviour
	{
		/// <summary>
		/// Camera used to raycast into the board.
		/// </summary>
		[SerializeField] private UnityEngine.Camera targetCamera;
		/// <summary>
		/// Layer mask used for board raycasts.
		/// </summary>
		[SerializeField] private LayerMask mask = -1;
		/// <summary>
		/// Maximum raycast distance.
		/// </summary>
		[SerializeField] private float maxDistance = 500f;
		/// <summary>
		/// Query trigger interaction for raycasts.
		/// </summary>
		[SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
		/// <summary>
		/// Tag used to identify board colliders.
		/// </summary>
		[SerializeField] private string boardTag = "Board";

		/// <summary>
		/// Event raised when the hovered cell changes.
		/// </summary>
		public event Action<Vector3Int> CellChanged;
		/// <summary>
		/// Event raised when the hover is cleared.
		/// </summary>
		public event Action HoverCleared;

		/// <summary>
		/// Gets the active raycast mask.
		/// </summary>
		public LayerMask Mask => mask;
		/// <summary>
		/// Gets the maximum raycast distance.
		/// </summary>
		public float MaxDistance => maxDistance;

		/// <summary>
		/// Whether the cursor currently hits the board.
		/// </summary>
		private bool hasBoardHit;
		/// <summary>
		/// Last resolved hovered cell.
		/// </summary>
		private Vector3Int lastCell;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Resolve a camera reference if one isn't assigned.
			if (targetCamera == null)
			{
				targetCamera = GetComponentInChildren<UnityEngine.Camera>();
			}
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Cast a ray and update hover state.
			if (!TryGetRay(out Ray ray))
			{
				ClearHover();
				return;
			}

			if (TryResolveBoardHit(ray, out RaycastHit hit))
			{
				Vector3Int cell = ResolveCell(hit);
				if (!hasBoardHit || cell != lastCell)
				{
					hasBoardHit = true;
					lastCell = cell;
					CellChanged?.Invoke(cell);
				}

				return;
			}

			ClearHover();
		}

		/// <summary>
		/// Updates the raycast layer mask.
		/// </summary>
		public void SetMask(LayerMask newMask)
		{
			// Store the new mask.
			mask = newMask;
		}

		/// <summary>
		/// Tries to build a ray from the current mouse position.
		/// </summary>
		public bool TryGetRay(out Ray ray)
		{
			// Build a screen-space ray from the mouse cursor.
			ray = default;

			if (targetCamera == null)
			{
				return false;
			}

			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return false;
			}

			Vector2 screenPosition = mouse.position.ReadValue();
			ray = targetCamera.ScreenPointToRay(screenPosition);
			return true;
		}

		/// <summary>
		/// Clears hover state and notifies listeners.
		/// </summary>
		private void ClearHover()
		{
			// Reset hover state and emit event.
			if (!hasBoardHit)
			{
				return;
			}

			hasBoardHit = false;
			HoverCleared?.Invoke();
		}

		/// <summary>
		/// Tries to resolve a board hit from a raycast.
		/// </summary>
		private bool TryResolveBoardHit(Ray ray, out RaycastHit resolvedHit)
		{
			// Raycast and select the closest hit on the board.
			resolvedHit = default;

			RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, mask, triggerInteraction);
			if (hits == null || hits.Length == 0)
			{
				return false;
			}

			Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider == null)
				{
					continue;
				}

				GameObject hitObject = hits[i].collider.gameObject;
				if (hitObject != null && !string.IsNullOrEmpty(boardTag) && hitObject.CompareTag(boardTag))
				{
					resolvedHit = hits[i];
					return true;
				}
			}

			if (string.IsNullOrEmpty(boardTag))
			{
				for (int i = 0; i < hits.Length; i++)
				{
					if (hits[i].collider == null)
					{
						continue;
					}

					resolvedHit = hits[i];
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Resolves a cell coordinate from a raycast hit point.
		/// </summary>
		private static Vector3Int ResolveCell(RaycastHit hit)
		{
			// Bias toward the hit surface to avoid boundary issues.
			const float epsilon = 0.001f;
			Vector3 biased = hit.point - (hit.normal * epsilon);
			return new Vector3Int(
				Mathf.FloorToInt(biased.x),
				Mathf.FloorToInt(biased.y),
				Mathf.FloorToInt(biased.z));
		}
	}
}
