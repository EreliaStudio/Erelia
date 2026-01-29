using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed = 5f;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction rotateAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null)
        {
            return;
        }

        moveAction = playerInput.actions.FindAction("Player/Move", false)
            ?? playerInput.actions.FindAction("Move", false);
        ResolveRotateAction();
    }

    private void OnEnable()
    {
        ApplyMoveLayoutOverride(moveAction);
        moveAction?.Enable();
        ApplyRotateLayoutOverride(rotateAction);
        rotateAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        rotateAction?.Disable();
    }

    private void Update()
    {
        if (cameraPivot == null || moveAction == null)
        {
            return;
        }

        Vector3 forward = cameraPivot.forward;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 input = forward * moveInput.y + right * moveInput.x;

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        transform.position += input * moveSpeed * Time.deltaTime;
    }

    private static void ApplyMoveLayoutOverride(InputAction action)
    {
        if (action == null || Keyboard.current == null)
        {
            return;
        }

        string layout = Keyboard.current.keyboardLayout ?? string.Empty;
        bool useAzerty = layout.IndexOf("azerty", StringComparison.OrdinalIgnoreCase) >= 0
            || layout.IndexOf("french", StringComparison.OrdinalIgnoreCase) >= 0;
        string up = useAzerty ? "<Keyboard>/z" : "<Keyboard>/w";
        string left = useAzerty ? "<Keyboard>/q" : "<Keyboard>/a";
        string down = "<Keyboard>/s";
        string right = "<Keyboard>/d";

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite)
            {
                continue;
            }

            switch (binding.name)
            {
                case "up":
                    action.ApplyBindingOverride(i, up);
                    break;
                case "left":
                    action.ApplyBindingOverride(i, left);
                    break;
                case "down":
                    action.ApplyBindingOverride(i, down);
                    break;
                case "right":
                    action.ApplyBindingOverride(i, right);
                    break;
            }
        }
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
        string negative = useAzerty ? "<Keyboard>/a" : "<Keyboard>/q";
        string positive = "<Keyboard>/e";

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isPartOfComposite)
            {
                continue;
            }

            switch (binding.name)
            {
                case "negative":
                    action.ApplyBindingOverride(i, negative);
                    break;
                case "positive":
                    action.ApplyBindingOverride(i, positive);
                    break;
            }
        }
    }
}
