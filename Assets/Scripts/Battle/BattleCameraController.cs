using UnityEngine;
using UnityEngine.InputSystem;

public class BattleCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed = 5f;
    private PlayerInput playerInput;
    private InputAction moveAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        ResolveMoveAction();
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
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
}
