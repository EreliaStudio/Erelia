using System;
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
        transform.localPosition = CameraLocalPosition;
        transform.LookAt(transform.parent != null
            ? transform.parent.TransformPoint(LookAtLocalPosition)
            : transform.TransformPoint(LookAtLocalPosition), Vector3.up);
    }

    private void OnEnable()
    {
        ResolveRotateAction();
        ApplyRotateLayoutOverride(rotateAction);
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

    private static void ApplyRotateLayoutOverride(InputAction action)
    {
        if (action == null || Keyboard.current == null)
        {
            return;
        }

        string layout = Keyboard.current.keyboardLayout ?? string.Empty;
        bool useAzerty = layout.IndexOf("azerty", StringComparison.OrdinalIgnoreCase) >= 0
            || layout.IndexOf("french", StringComparison.OrdinalIgnoreCase) >= 0;
        string negativePath = useAzerty ? "<Keyboard>/a" : "<Keyboard>/q";

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (binding.isPartOfComposite && binding.name == "negative")
            {
                action.ApplyBindingOverride(i, negativePath);
                break;
            }
        }
    }
}
