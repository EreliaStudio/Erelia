using UnityEngine;

public abstract class BattlePhaseBase : IBattlePhase
{
    public BattleContext battleContext = null;
	
    public abstract BattlePhase Phase { get; }
    public abstract void OnEntry();
    public abstract void OnExit();
}
