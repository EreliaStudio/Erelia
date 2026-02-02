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
            Vector3Int cell = ResolveCell(hit);

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
