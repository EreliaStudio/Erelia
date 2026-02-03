using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private TrainerData trainerData = new TrainerData();

    public PlayerController Controller => controller;
    public TrainerData TrainerData => trainerData;

    private void Awake()
    {
        ResolveController();
        ApplyControllerConfig();
        trainerData?.Initialize();
    }

    private void Reset()
    {
        ResolveController();
        ApplyControllerConfig();
    }

    private void OnValidate()
    {
        ResolveController();
        ApplyControllerConfig();
    }

    private void ResolveController()
    {
        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (controller == null)
        {
            controller = gameObject.AddComponent<PlayerController>();
        }
    }

    private void ApplyControllerConfig()
    {
        if (controller == null)
        {
            return;
        }

        controller.Configure(cameraPivot, moveSpeed);
    }
}
