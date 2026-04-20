using System;
using UnityEngine;

public sealed class ResolveActionPhase : BattlePhase
{
	public ResolveActionPhase(BattleContext p_context) : base(p_context)
	{
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.ResolveAction;

	public event Action Resolved;

	public override void Enter()
	{
		Execute(Context.PendingAction);
		Context.PendingAction = null;
		Resolved?.Invoke();
	}

	private void Execute(BattleAction p_action)
	{
		if (p_action?.SourceUnit == null)
		{
			return;
		}

		switch (p_action)
		{
			case MoveAction moveAction:
				ExecuteMove(moveAction);
				break;

			case AbilityAction abilityAction:
				ExecuteAbility(abilityAction);
				break;

			case EndTurnAction:
				break;
		}

		p_action.SourceUnit.BattleAttributes.TurnBar.SetCurrent(0f);
	}

	private void ExecuteMove(MoveAction p_action)
	{
		if (p_action == null || p_action.SourceUnit == null)
		{
			return;
		}

		int availableMovement = p_action.SourceUnit.BattleAttributes.MovementPoints.Current;
		if (availableMovement <= 0)
		{
			return;
		}

		if (!Context.Board.TryGetPosition(p_action.SourceUnit, out Vector3Int currentPosition))
		{
			return;
		}

		int travelCost = Mathf.Abs(p_action.Destination.x - currentPosition.x) + Mathf.Abs(p_action.Destination.y - currentPosition.y) + Mathf.Abs(p_action.Destination.z - currentPosition.z);
		if (travelCost <= 0 || travelCost > availableMovement)
		{
			return;
		}

		if (!Context.Board.TryMove(p_action.SourceUnit, p_action.Destination))
		{
			return;
		}

		p_action.SourceUnit.BattleAttributes.MovementPoints.Decrease(travelCost);
	}

	private void ExecuteAbility(AbilityAction p_action)
	{
		if (p_action?.Ability == null || p_action.SourceUnit == null || !CanAfford(p_action.SourceUnit, p_action.Ability))
		{
			return;
		}

		p_action.SourceUnit.BattleAttributes.ActionPoints.Decrease(p_action.Ability.Cost.Ability);
		p_action.SourceUnit.BattleAttributes.MovementPoints.Decrease(p_action.Ability.Cost.Movement);

		for (int targetIndex = 0; targetIndex < p_action.Targets.Count; targetIndex++)
		{
			BattleObject target = p_action.Targets[targetIndex];
			for (int effectIndex = 0; effectIndex < p_action.Ability.Effects.Count; effectIndex++)
			{
				Effect effect = p_action.Ability.Effects[effectIndex];
				effect?.Apply(p_action.SourceUnit, target, Context);
			}
		}
	}

	private static bool CanAfford(BattleUnit p_unit, Ability p_ability)
	{
		return p_unit.BattleAttributes.ActionPoints.Current >= p_ability.Cost.Ability &&
			p_unit.BattleAttributes.MovementPoints.Current >= p_ability.Cost.Movement;
	}
}
