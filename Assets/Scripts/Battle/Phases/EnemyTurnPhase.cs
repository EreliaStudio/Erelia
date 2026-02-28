namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class EnemyTurnPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.EnemyTurn;
	}
}
