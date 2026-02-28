namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class CleanupPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.Cleanup;
	}
}
