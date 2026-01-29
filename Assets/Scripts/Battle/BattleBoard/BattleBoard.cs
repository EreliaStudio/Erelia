using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    [SerializeField] private BattleBoardData data = new BattleBoardData();
    [SerializeField] private BattleBoardView view = new BattleBoardView();

    public BattleBoardData Data => data;
    public BattleBoardView View => view;

    private bool initialized;

    private void Awake()
    {
        EnsureInstances();
        InitializeView();
    }

    private void OnValidate()
    {
        EnsureInstances();
        InitializeView();
    }

    private void Start()
    {
        InitializeFromRequest();
    }

    private void OnDestroy()
    {
        view?.Cleanup(); 
    }

    public void Initialize(BattleBoardData boardData, VoxelRegistry registryValue)
    {
        data = boardData;
        data?.EnsureInitialized();

        initialized = true;
        if (view != null && data != null)
        {
            view.Initialize(data, registryValue, transform);
            view.Build(data, registryValue);
        }
    }

    public void InitializeFromRequest()
    {
        if (initialized)
        {
            return;
        }

        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            return;
        }

        Initialize(request.BattleBoard, request.Registry);
    }

    public void RebuildMask()
    {
        if (view != null)
        {
            view.RebuildMask();
        }
    }

    private void EnsureInstances()
    {
        if (data == null)
        {
            data = new BattleBoardData();
        }

        if (view == null)
        {
            view = new BattleBoardView();
        }
    }

    private void InitializeView()
    {
        if (view == null)
        {
            return;
        }

        data?.EnsureInitialized();
        view.Initialize(data, BattleRequestStore.Current?.Registry, transform);
    }
}
