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

		public event System.Action<Vector3Int, RaycastHit> CellHovered;
		public event System.Action HoverCleared;

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

			if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask, triggerInteraction))
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

		private static Vector3Int ResolveCell(RaycastHit hit)
		{
			Debug.Log("Base input value : " + hit.point);
			const float epsilon = 0.001f;
			Vector3 biased = hit.point - (hit.normal * epsilon);
			return new Vector3Int(
				Mathf.FloorToInt(biased.x),
				Mathf.FloorToInt(biased.y),
				Mathf.FloorToInt(biased.z));
		}
	}
}
