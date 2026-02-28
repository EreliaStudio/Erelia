namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class ResolveActionPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.ResolveAction;
	}
}
