public interface IBattlePhase
{
	BattlePhaseType PhaseType { get; }

	void Bind(BattleOrchestrator orchestrator);
	void Enter();
	void Exit();
}
