public interface IBattlePhase
{
    BattlePhase Phase { get; }
    void OnEntry();
    void OnExit();
}
