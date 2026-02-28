using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player.Camera
{
	public sealed class MouseBoardCellCursor : MonoBehaviour
	{
		[SerializeField] private UnityEngine.Camera targetCamera;
		[SerializeField] private LayerMask mask = -1;
		[SerializeField] private float maxDistance = 500f;
		[SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
		[SerializeField] private string boardTag = "Board";

		public event Action<Vector3Int> CellChanged;
		public event Action HoverCleared;

		public LayerMask Mask => mask;
		public float MaxDistance => maxDistance;

		private bool hasBoardHit;
		private Vector3Int lastCell;

		private void Awake()
		{
			if (targetCamera == null)
			{
				targetCamera = GetComponentInChildren<UnityEngine.Camera>();
			}
		}

		private void Update()
		{
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

		public void SetMask(LayerMask newMask)
		{
			mask = newMask;
		}

		public bool TryGetRay(out Ray ray)
		{
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

		private void ClearHover()
		{
			if (!hasBoardHit)
			{
				return;
			}

			hasBoardHit = false;
			HoverCleared?.Invoke();
		}

		private bool TryResolveBoardHit(Ray ray, out RaycastHit resolvedHit)
		{
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

		private static Vector3Int ResolveCell(RaycastHit hit)
		{
			const float epsilon = 0.001f;
			Vector3 biased = hit.point - (hit.normal * epsilon);
			return new Vector3Int(
				Mathf.FloorToInt(biased.x),
				Mathf.FloorToInt(biased.y),
				Mathf.FloorToInt(biased.z));
		}
	}
}
