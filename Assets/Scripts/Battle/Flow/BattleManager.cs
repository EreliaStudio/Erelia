using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private BattlePlacementPhase placementPhase = new BattlePlacementPhase();
    [SerializeField] private BattleContext battleContext = new BattleContext();

    public BattleRequest CurrentRequest { get; private set; }
    private IBattlePhase currentPhase;

    public void Initialize(BattleRequest request)
    {
        CurrentRequest = request;
        Debug.Log("BattleManager: Initialize");

        EnterPhase(BattlePhase.Placement);
    }

    public void EnterPhase(BattlePhase phase)
    {
		if (currentPhase != null)
		{	
        	Debug.Log($"BattleManager: ExitingPhase {currentPhase?.Phase}");
		}
        currentPhase?.OnExit();

        currentPhase = ResolvePhase(phase);
        Debug.Log($"BattleManager: EnterPhase {phase}");

        if (currentPhase is BattlePhaseBase phaseBase)
        {
            phaseBase.battleContext = battleContext;
        }
        currentPhase?.OnEntry();
    }

    private IBattlePhase ResolvePhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.Placement:
                return placementPhase;
            default:
                return null;
        }
    }
}
