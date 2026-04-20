using System;

public sealed class TurnIdlePhase : BattlePhase
{
	public TurnIdlePhase(BattleContext p_context) : base(p_context)
	{
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.TurnIdle;

	public event Action<BattleUnit> UnitReady;

	public override void Tick(float p_deltaTime)
	{
		if (p_deltaTime <= 0f)
		{
			return;
		}

		for (int index = 0; index < Context.AllUnits.Count; index++)
		{
			BattleUnit unit = Context.AllUnits[index];
			if (unit == null || unit.IsDefeated || unit.IsTurnReady)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.Increase(p_deltaTime);
			if (!unit.IsTurnReady)
			{
				continue;
			}

			Context.ActiveUnit = unit;
			UnitReady?.Invoke(unit);
			return;
		}
	}
}
