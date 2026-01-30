using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    private static readonly Vector3 CameraLocalPosition = new Vector3(-10f, 10f, -10f);
    private static readonly Vector3 LookAtLocalPosition = Vector3.zero;
    [SerializeField] private float mouseOrbitSensitivity = 0.75f;
    [SerializeField] private float keyboardOrbitSpeed = 90f;
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float minZoomMultiplier = 0.5f;
    [SerializeField] private float maxZoomMultiplier = 2.5f;
    private PlayerInput playerInput;
    private InputAction rotateAction;
    private float baseZoomDistance;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
    }

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        transform.localPosition = request != null ? request.CameraLocalPosition : CameraLocalPosition;
        baseZoomDistance = Mathf.Max(0.01f, transform.localPosition.magnitude);
        transform.LookAt(transform.parent != null
            ? transform.parent.TransformPoint(LookAtLocalPosition)
            : transform.TransformPoint(LookAtLocalPosition), Vector3.up);
    }

    private void OnEnable()
    {
        ResolveRotateAction();
        rotateAction?.Enable();
    }

    private void OnDisable()
    {
        rotateAction?.Disable();
    }

    private void LateUpdate()
    {
        float rotateInput = rotateAction != null ? rotateAction.ReadValue<float>() : 0f;
        if (Mathf.Abs(rotateInput) > 0.01f)
        {
            Vector3 pivot = transform.parent != null
                ? transform.parent.TransformPoint(LookAtLocalPosition)
                : transform.TransformPoint(LookAtLocalPosition);
            transform.RotateAround(pivot, Vector3.up, rotateInput * keyboardOrbitSpeed * Time.deltaTime);
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 current = transform.localPosition;
            float distance = current.magnitude;
            if (distance > 0.0001f)
            {
                float scrollSteps = scroll / 120f;
                float scale = 1f - scrollSteps * zoomSpeed;
                float minDistance = baseZoomDistance * minZoomMultiplier;
                float maxDistance = baseZoomDistance * maxZoomMultiplier;
                float newDistance = Mathf.Clamp(distance * scale, minDistance, maxDistance);
                transform.localPosition = current.normalized * newDistance;
            }
        }

        if (mouse.rightButton.isPressed)
        {
            float mouseX = mouse.delta.ReadValue().x;
            if (Mathf.Abs(mouseX) > 0.01f)
            {
                Vector3 pivot = transform.parent != null
                    ? transform.parent.TransformPoint(LookAtLocalPosition)
                    : transform.TransformPoint(LookAtLocalPosition);
                transform.RotateAround(pivot, Vector3.up, mouseX * mouseOrbitSensitivity);
            }
        }

        Vector3 lookPoint = transform.parent != null
            ? transform.parent.TransformPoint(LookAtLocalPosition)
            : transform.TransformPoint(LookAtLocalPosition);
        transform.LookAt(lookPoint, Vector3.up);
    }

    private void ResolveRotateAction()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            rotateAction = null;
            return;
        }

        InputAction actionFromMap = playerInput.currentActionMap != null
            ? playerInput.currentActionMap.FindAction("RotateCamera", false)
            : null;

        rotateAction = actionFromMap
            ?? playerInput.actions.FindAction("Player/RotateCamera", false)
            ?? playerInput.actions.FindAction("RotateCamera", false);
    }

}
