using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private BattlePlacementPhase placementPhase = null;
    [SerializeField] private BattleBoard battleBoard = null;
    [SerializeField] private bool clearPlacementOnExit = true;
    [Header("Mask Rendering")]
    [SerializeField] private BattleCellRenderMeshBuilder maskRenderBuilder = new BattleCellRenderMeshBuilder();
    [SerializeField] private BattleCellCollisionMeshBuilder maskCollisionBuilder = new BattleCellCollisionMeshBuilder();

    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.None;
    public BattleRequest CurrentRequest { get; private set; }

    public void Initialize(BattleRequest request)
    {
        CurrentRequest = request;
        if (placementPhase == null)
        {
            placementPhase = FindFirstObjectByType<BattlePlacementPhase>(FindObjectsInactive.Include);
        }

        if (battleBoard == null)
        {
            battleBoard = FindFirstObjectByType<BattleBoard>(FindObjectsInactive.Include);
        }

        if (battleBoard != null)
        {
            battleBoard.View.ConfigureMaskBuilders(maskRenderBuilder, maskCollisionBuilder);
        }

        EnterPhase(BattlePhase.Placement);
    }

    public void EnterPhase(BattlePhase phase)
    {
        if (phase == CurrentPhase)
        {
            return;
        }

        ExitPhase(CurrentPhase);
        CurrentPhase = phase;

        switch (phase)
        {
            case BattlePhase.Placement:
                placementPhase?.BuildPlacementMask();
                RefreshMaskViews();
                break;
            case BattlePhase.Action:
                break;
            case BattlePhase.Resolution:
                break;
            case BattlePhase.End:
                break;
        }
    }

    public void RefreshMaskViews()
    {
        if (battleBoard != null)
        {
            battleBoard.RebuildMask();
            return;
        }
    }

    private void ExitPhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Placement:
                if (clearPlacementOnExit)
                {
                    placementPhase?.ClearPlacementMask();
                    RefreshMaskViews();
                }
                break;
        }
    }
}
