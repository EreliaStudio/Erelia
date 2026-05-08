using System.Collections.Generic;

public sealed class TamingService
{
	private readonly GameContext gameContext;
	private readonly List<WildBattleUnit> trackedWildUnits = new();
	private readonly List<CreatureUnit> impressedRecruits = new();

	private BattleContext activeBattleContext;

	public TamingService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public void Initialize()
	{
		EventCenter.BattleStarted += OnBattleStarted;
		EventCenter.BattleEventOccurred += OnBattleEventOccurred;
		EventCenter.BattleResolved += OnBattleResolved;
	}

	public void Shutdown()
	{
		EventCenter.BattleStarted -= OnBattleStarted;
		EventCenter.BattleEventOccurred -= OnBattleEventOccurred;
		EventCenter.BattleResolved -= OnBattleResolved;
		ClearBattleState();
	}

	private void OnBattleStarted(BattleContext p_battleContext)
	{
		ClearBattleState();
		activeBattleContext = p_battleContext;

		if (activeBattleContext?.EnemyUnits == null)
		{
			return;
		}

		for (int index = 0; index < activeBattleContext.EnemyUnits.Count; index++)
		{
			if (activeBattleContext.EnemyUnits[index] is WildBattleUnit wildUnit &&
				wildUnit.TamingProgress != null)
			{
				trackedWildUnits.Add(wildUnit);
			}
		}
	}

	private void OnBattleEventOccurred(BattleEvent p_featEvent)
	{
		if (p_featEvent?.Caster == null ||
			p_featEvent.Caster.Side != BattleSide.Player)
		{
			return;
		}

		for (int index = 0; index < trackedWildUnits.Count; index++)
		{
			WildBattleUnit wildUnit = trackedWildUnits[index];
			if (wildUnit == null || !wildUnit.IsActiveInBattle || wildUnit.IsTamed)
			{
				continue;
			}

			wildUnit.EvaluateTamingEvent(p_featEvent);
			if (!wildUnit.IsTamed)
			{
				continue;
			}

			CreatureUnit recruit = TamingRules.CreateRecruitFromImpressedUnit(wildUnit);
			if (recruit != null)
			{
				impressedRecruits.Add(recruit);
			}

			EventCenter.EmitCreatureImpressed(activeBattleContext, wildUnit);
			EventCenter.EmitBattleUnitRemovalRequested(activeBattleContext, wildUnit);
		}
	}

	private void OnBattleResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		if (!ReferenceEquals(p_battleContext, activeBattleContext))
		{
			return;
		}

		if (p_winner == BattleSide.Player && impressedRecruits.Count > 0)
		{
			EventCenter.EmitTamingResolved(
				activeBattleContext,
				new List<CreatureUnit>(impressedRecruits));
		}

		ClearBattleState();
	}

	private void ClearBattleState()
	{
		activeBattleContext = null;
		trackedWildUnits.Clear();
		impressedRecruits.Clear();
	}
}
