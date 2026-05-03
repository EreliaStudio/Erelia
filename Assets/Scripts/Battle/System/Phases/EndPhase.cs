using System.Collections.Generic;

public sealed class EndPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.End;

	public override void Enter()
	{
		BattleOutcomeRules.TryComputeOutcome(BattleContext, out BattleOutcome outcome);

		ApplyFeatProgressionToPlayerUnits(BattleContext, outcome?.Winner == BattleSide.Player);

		EventCenter.EmitBattleEnded(outcome);
	}

	private static void ApplyFeatProgressionToPlayerUnits(BattleContext battleContext, bool includeTransientRequirements)
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

			if (includeTransientRequirements)
			{
				unit.RecordFeatEvent(new WinBattleCountRequirement.Event { });
			}

			if (!unit.IsDefeated)
			{
				unit.RecordFeatEvent(new SurviveBattleCountRequirement.Event { });
			}

			IReadOnlyList<FeatRequirement.EventBase> events = unit.PendingFeatEvents;
			FeatProgressionService.RegisterFightEvents(unit.SourceUnit, events, includeTransientRequirements);
		}
	}
}
