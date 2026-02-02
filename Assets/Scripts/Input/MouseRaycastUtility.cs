using UnityEngine;
using UnityEngine.InputSystem;

public static class MouseRaycastUtility
{
    public static bool TryGetHit(
        Camera camera,
        LayerMask mask,
        float maxDistance,
        QueryTriggerInteraction triggerInteraction,
        out RaycastHit hit)
    {
        hit = default;
        if (camera == null)
        {
            return false;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        Ray ray = camera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out hit, maxDistance, mask, triggerInteraction);
    }

    public static bool TryGetHit(
        Camera camera,
        LayerMask mask,
        out RaycastHit hit)
    {
        return TryGetHit(camera, mask, 500f, QueryTriggerInteraction.Ignore, out hit);
    }

    public static bool TryGetWorldPoint(
        Camera camera,
        LayerMask mask,
        float maxDistance,
        QueryTriggerInteraction triggerInteraction,
        out Vector3 worldPoint)
    {
        if (TryGetHit(camera, mask, maxDistance, triggerInteraction, out RaycastHit hit))
        {
            worldPoint = hit.point;
            return true;
        }

        worldPoint = default;
        return false;
    }

    public static bool TryGetWorldPoint(
        Camera camera,
        LayerMask mask,
        out Vector3 worldPoint)
    {
        return TryGetWorldPoint(camera, mask, 500f, QueryTriggerInteraction.Ignore, out worldPoint);
    }
}
