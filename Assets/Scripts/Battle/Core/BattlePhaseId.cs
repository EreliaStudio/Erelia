namespace Erelia.Battle
{
	public enum BattlePhaseId
	{
		None = 0,
		Initialize = 1,
		Placement = 2,
		PlayerTurn = 3,
		EnemyTurn = 4,
		ResolveAction = 5,
		Victory = 6,
		Defeat = 7,
		Cleanup = 8
	}
}
