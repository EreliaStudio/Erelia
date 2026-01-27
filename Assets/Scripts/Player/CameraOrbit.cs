using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    private static readonly Vector3 CameraLocalPosition = new Vector3(10f, 10f, 10f);
    private static readonly Vector3 LookAtLocalPosition = Vector3.zero;
    private const float OrbitSensitivity = 2.5f;

    private void Start()
    {
        transform.localPosition = CameraLocalPosition;
        transform.LookAt(transform.parent != null
            ? transform.parent.TransformPoint(LookAtLocalPosition)
            : transform.TransformPoint(LookAtLocalPosition), Vector3.up);
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
                Vector3 pivot = transform.parent != null
                    ? transform.parent.TransformPoint(LookAtLocalPosition)
                    : transform.TransformPoint(LookAtLocalPosition);
                transform.RotateAround(pivot, Vector3.up, mouseX * OrbitSensitivity);
            }
        }

        Vector3 lookPoint = transform.parent != null
            ? transform.parent.TransformPoint(LookAtLocalPosition)
            : transform.TransformPoint(LookAtLocalPosition);
        transform.LookAt(lookPoint, Vector3.up);
    }
}
