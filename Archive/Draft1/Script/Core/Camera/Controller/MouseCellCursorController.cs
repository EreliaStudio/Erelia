using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Camera.Controller
{
	public class MouseCellCursorController : MonoBehaviour
	{
		[SerializeField] private global::UnityEngine.Camera targetCamera = null;
		[SerializeField] private LayerMask mask = -1;
		[SerializeField] private float maxDistance = 500f;
		[SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
		private const string BoardTag = "Board";
		private const string CreatureTag = "Creature";

		public event System.Action<Vector3Int, RaycastHit> CellHovered;
		public event System.Action HoverCleared;
		public LayerMask Mask => mask;
		public float MaxDistance => maxDistance;

		private bool hasHit;
		private Vector3Int lastCell;

		private void Awake()
		{
			if (targetCamera == null)
			{
				targetCamera = GetComponentInChildren<global::UnityEngine.Camera>();
			}
		}

		public void SetMask(LayerMask mask)
		{
			this.mask = mask;
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

		private void Update()
		{
			if (targetCamera == null)
			{
				return;
			}

			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return;
			}

			Vector2 screenPosition = mouse.position.ReadValue();
			Ray ray = targetCamera.ScreenPointToRay(screenPosition);

			if (TryResolveHit(ray, out RaycastHit hit))
			{
				Vector3Int cell = ResolveCell(hit);

				if (!hasHit || cell != lastCell)
				{
					hasHit = true;
					lastCell = cell;
					CellHovered?.Invoke(cell, hit);
				}

				return;
			}

			if (hasHit)
			{
				hasHit = false;
				HoverCleared?.Invoke();
			}
		}

		private bool TryResolveHit(Ray ray, out RaycastHit resolvedHit)
		{
			resolvedHit = default;

			RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, mask, triggerInteraction);
			if (hits == null || hits.Length == 0)
			{
				return false;
			}

			System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

			RaycastHit? boardHit = null;
			RaycastHit? firstHit = null;
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider == null)
				{
					continue;
				}

				if (!firstHit.HasValue)
				{
					firstHit = hits[i];
				}

				GameObject hitObject = hits[i].collider.gameObject;
				if (hitObject != null && hitObject.CompareTag(BoardTag))
				{
					boardHit = hits[i];
					break;
				}
			}

			if (!firstHit.HasValue)
			{
				return false;
			}

			GameObject firstObject = firstHit.Value.collider != null ? firstHit.Value.collider.gameObject : null;
			if (firstObject != null && firstObject.CompareTag(CreatureTag))
			{
				if (boardHit.HasValue)
				{
					resolvedHit = boardHit.Value;
					return true;
				}

				return false;
			}

			resolvedHit = firstHit.Value;
			return true;
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
