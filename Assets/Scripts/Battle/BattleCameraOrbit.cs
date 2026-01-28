using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCameraOrbit : MonoBehaviour
{
    private static readonly Vector3 DefaultCameraLocalPosition = new Vector3(-10f, 10f, -10f);
    private static readonly Vector3 LookAtLocalPosition = Vector3.zero;
    private const float OrbitSensitivity = 2.5f;

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        transform.localPosition = request != null ? request.CameraLocalPosition : DefaultCameraLocalPosition;
        transform.LookAt(GetLookPoint(), Vector3.up);
    }

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.rightButton.isPressed)
        {
            float mouseX = mouse.delta.ReadValue().x;
            if (Mathf.Abs(mouseX) > 0.01f)
            {
                Vector3 pivot = GetLookPoint();
                transform.RotateAround(pivot, Vector3.up, mouseX * OrbitSensitivity);
            }
        }

        transform.LookAt(GetLookPoint(), Vector3.up);
    }

    private Vector3 GetLookPoint()
    {
        if (transform.parent == null)
        {
            return transform.TransformPoint(LookAtLocalPosition);
        }

        return transform.parent.TransformPoint(LookAtLocalPosition);
    }
}
