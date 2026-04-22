using UnityEngine;

public sealed class SetupPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Setup;

	public override void Enter()
	{
		if (BattleContext == null)
		{
			return;
		}

		if (!BattleContext.HasLivingUnits(BattleSide.Player) || !BattleContext.HasLivingUnits(BattleSide.Enemy))
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		InitializeTurnBars();
		Coordinator.TransitionTo(BattlePhaseType.Placement);
	}

	private void InitializeTurnBars()
	{
		for (int index = 0; index < BattleContext.AllUnits.Count; index++)
		{
			BattleUnit unit = BattleContext.AllUnits[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			float max = unit.BattleAttributes.TurnBar.Max;
			float initial = Random.Range(0f, max);
			unit.BattleAttributes.TurnBar.SetCurrent(initial);
		}
	}
}
