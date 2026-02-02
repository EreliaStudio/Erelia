using UnityEngine;

public static class LayerUtils
{
    private const string MouseSurfaceLayerName = "MouseSurface";
    private static int mouseSurfaceLayer = -2;

    public static void ApplyMouseSurfaceLayer(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (mouseSurfaceLayer == -2)
        {
            mouseSurfaceLayer = LayerMask.NameToLayer(MouseSurfaceLayerName);
            if (mouseSurfaceLayer == -1)
            {
                Debug.LogError("Layer not found: " + MouseSurfaceLayerName);
                return;
            }
        }

        target.layer = mouseSurfaceLayer;
    }
}
