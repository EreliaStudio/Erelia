using System;
using System.Collections.Generic;

public sealed class EnemyTurnPhase : BattlePhase
{
	public EnemyTurnPhase(BattleContext p_context) : base(p_context)
	{
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.EnemyTurn;

	public event Action<BattleAction> ActionChosen;

	public override void Enter()
	{
		BattleUnit activeUnit = Context.ActiveUnit;
		if (activeUnit == null || activeUnit.IsDefeated)
		{
			return;
		}

		Context.RefillTurnResources(activeUnit);

		if (TryBuildAbilityAction(activeUnit, out BattleAction action))
		{
			ActionChosen?.Invoke(action);
			return;
		}

		ActionChosen?.Invoke(new EndTurnAction(activeUnit));
	}

	private bool TryBuildAbilityAction(BattleUnit p_activeUnit, out BattleAction p_action)
	{
		p_action = null;
		if (p_activeUnit?.Abilities == null)
		{
			return false;
		}

		for (int index = 0; index < p_activeUnit.Abilities.Count; index++)
		{
			Ability ability = p_activeUnit.Abilities[index];
			if (ability == null || !CanAfford(p_activeUnit, ability))
			{
				continue;
			}

			if (!Context.TryGetFirstLivingOpponent(p_activeUnit, out BattleUnit targetUnit))
			{
				continue;
			}

			p_action = new AbilityAction(p_activeUnit, ability, new List<BattleObject> { targetUnit });
			return true;
		}

		return false;
	}

	private static bool CanAfford(BattleUnit p_unit, Ability p_ability)
	{
		return p_unit != null &&
			p_ability != null &&
			p_unit.BattleAttributes.ActionPoints.Current >= p_ability.Cost.Ability &&
			p_unit.BattleAttributes.MovementPoints.Current >= p_ability.Cost.Movement;
	}
}
