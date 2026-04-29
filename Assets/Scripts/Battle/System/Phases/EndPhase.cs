using System.Collections.Generic;

public sealed class EndPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.End;

	public override void Enter()
	{
		BattleOutcomeRules.TryComputeOutcome(BattleContext, out BattleOutcome outcome);

		if (outcome?.Winner == BattleSide.Player)
		{
			ApplyFeatProgressionToPlayerUnits(BattleContext);
		}

		EventCenter.EmitBattleEnded(outcome);
	}

	private static void ApplyFeatProgressionToPlayerUnits(BattleContext battleContext)
	{
		if (battleContext == null)
		{
			return;
		}

		IReadOnlyList<BattleUnit> playerUnits = battleContext.PlayerUnits;
		for (int unitIndex = 0; unitIndex < playerUnits.Count; unitIndex++)
		{
			BattleUnit unit = playerUnits[unitIndex];
			if (unit?.SourceUnit == null)
			{
				continue;
			}

			IReadOnlyList<FeatRequirement.EventBase> events = unit.PendingFeatEvents;
			for (int eventIndex = 0; eventIndex < events.Count; eventIndex++)
			{
				FeatProgressionService.RegisterEvent(unit.SourceUnit, events[eventIndex]);
			}
		}
	}
}
