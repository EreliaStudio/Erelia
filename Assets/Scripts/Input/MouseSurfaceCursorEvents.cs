using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSurfaceCursorEvents : MonoBehaviour
{
    [SerializeField] private Camera targetCamera = null;
	[SerializeField] private LayerMask mask = -1;

	private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
	private float maxDistance = 500.0f;

    public event System.Action<Vector3Int, RaycastHit> MoveMouseCursor;
    public event System.Action MouseLeaveModel;

    private bool hasHit;
    private Vector3Int lastCell;

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

        if (MouseRaycastUtility.TryGetHit(targetCamera, mask, maxDistance, triggerInteraction, out RaycastHit hit))
        {
            Vector3Int cell = new Vector3Int(
                Mathf.FloorToInt(hit.point.x),
                Mathf.FloorToInt(hit.point.y),
                Mathf.FloorToInt(hit.point.z));

            if (!hasHit || cell != lastCell)
            {
                hasHit = true;
                lastCell = cell;
                MoveMouseCursor?.Invoke(cell, hit);
            }
            return;
        }

        if (hasHit)
        {
            hasHit = false;
            MouseLeaveModel?.Invoke();
        }
    }
}
