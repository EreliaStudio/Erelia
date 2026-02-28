namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class VictoryPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.Victory;
	}
}
