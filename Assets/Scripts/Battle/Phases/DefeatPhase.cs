namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class DefeatPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.Defeat;
	}
}
