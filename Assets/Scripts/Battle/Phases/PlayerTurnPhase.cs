namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class PlayerTurnPhase : BattlePhase
	{
		public override BattlePhaseId Id => BattlePhaseId.PlayerTurn;
	}
}
