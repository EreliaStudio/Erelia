using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSurfaceCursorEvents : MonoBehaviour
{
    [SerializeField] private Camera targetCamera = null;
	[SerializeField] private LayerMask mask = -1;

	private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
	private float maxDistance = 500.0f;

    private void Update()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);
        // Debug.Log(
        //     $"MouseSurface TryGetHit params cam={targetCamera.name} camPos={targetCamera.transform.position} " +
        //     $"mouse={screenPosition} " +
        //     $"mask={mask.value} maxDistance={maxDistance} trigger={triggerInteraction} " +
        //     $"screen={screenPosition} rayOrigin={ray.origin} rayDir={ray.direction}");

        if (MouseRaycastUtility.TryGetHit(targetCamera, mask, maxDistance, triggerInteraction, out RaycastHit hit))
        {
            Collider collider = hit.collider;
            string colliderName = collider != null ? collider.name : "<null>";
            int layer = collider != null ? collider.gameObject.layer : -1;
            string layerName = collider != null ? LayerMask.LayerToName(layer) : "<none>";
            Debug.Log(
                $"MouseSurface hit {colliderName} layer={layerName} " +
                $"point={hit.point} normal={hit.normal} distance={hit.distance}");
        }
        else
        {
            Debug.Log("MouseSurface no hit");
        }
    }
}
