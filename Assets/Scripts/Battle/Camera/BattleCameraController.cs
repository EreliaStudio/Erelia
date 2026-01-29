using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mousePanSensitivity = 2f;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction rotateAction;
    private HashSet<Vector2Int> allowedCells;
    private Vector3Int originCell;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            return;
        }

        BattleBoardData board = request.BattleBoard;
        int radius = Mathf.Max(0, (board.SizeX - 1) / 2);
        int cornerRadius = Mathf.Max(1, Mathf.CeilToInt(radius / 3f));
        allowedCells = RoundedSquareShapeGenerator.BuildCells(radius, cornerRadius);
        originCell = board.OriginCell + new Vector3Int(radius, 0, radius);
    }

    private void OnEnable()
    {
        ResolveMoveAction();
        ResolveRotateAction();
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

        Vector3 keyboardDelta = input * moveSpeed * Time.deltaTime;
        Vector3 current = transform.position;
        current = ApplyClampedMove(current, current + keyboardDelta);

        Vector3 mouseDelta = Vector3.zero;
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.middleButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            Vector3 pan = forward * -delta.y + right * -delta.x;
            mouseDelta = pan * mousePanSensitivity;
        }

        if (mouseDelta.sqrMagnitude > 0.0001f)
        {
            const float maxMouseStep = 0.5f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(mouseDelta.magnitude / maxMouseStep));
            Vector3 step = mouseDelta / steps;
            for (int i = 0; i < steps; i++)
            {
                Vector3 desired = current + step;
                current = ApplyClampedMove(current, desired);
            }
        }

        transform.position = current;
    }

    private void ResolveMoveAction()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            moveAction = null;
            return;
        }

        InputAction actionFromMap = playerInput.currentActionMap != null
            ? playerInput.currentActionMap.FindAction("Move", false)
            : null;

        moveAction = actionFromMap
            ?? playerInput.actions.FindAction("Player/Move", false)
            ?? playerInput.actions.FindAction("Move", false);
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

    private bool IsInsideAllowedCells(Vector3 worldPosition)
    {
        Vector3Int cell = Vector3Int.FloorToInt(worldPosition);
        int offsetX = cell.x - originCell.x;
        int offsetZ = cell.z - originCell.z;
        return allowedCells.Contains(new Vector2Int(offsetX, offsetZ));
    }

    private Vector3 ApplyClampedMove(Vector3 current, Vector3 desired)
    {
        if (allowedCells == null || allowedCells.Count == 0)
        {
            return desired;
        }

        Vector3 next = current;
        Vector3 moveX = new Vector3(desired.x, current.y, current.z);
        if (IsInsideAllowedCells(moveX))
        {
            next.x = desired.x;
        }

        Vector3 moveZ = new Vector3(next.x, current.y, desired.z);
        if (IsInsideAllowedCells(moveZ))
        {
            next.z = desired.z;
        }

        return next;
    }
}
