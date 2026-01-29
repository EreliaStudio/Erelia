using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    private static readonly Vector3 CameraLocalPosition = new Vector3(-10f, 10f, -10f);
    private static readonly Vector3 LookAtLocalPosition = Vector3.zero;
    private const float OrbitSensitivity = 2.5f;
    [SerializeField] private float keyboardOrbitSpeed = 90f;
    private PlayerInput playerInput;
    private InputAction rotateAction;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
    }

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        transform.localPosition = request != null ? request.CameraLocalPosition : CameraLocalPosition;
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
