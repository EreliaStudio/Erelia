using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerTurnPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.PlayerTurn;

	public bool CanMoveTo(Vector3Int destination)
	{
		return BattleActionValidator.CanMoveTo(BattleContext, TurnContext, destination);
	}

	public IReadOnlyList<Vector3Int> GetReachableCells()
	{
		return BattleActionValidator.GetReachableCells(BattleContext, TurnContext);
	}

	public bool CanUseAbility(Ability ability)
	{
		return BattleActionValidator.CanUseAbility(BattleContext, TurnContext, ability);
	}

	public AbilityCastLegality GetCastLegality(Ability ability, Vector3Int targetCell)
	{
		return BattleActionValidator.GetCastLegality(BattleContext, TurnContext, ability, targetCell);
	}

	public IReadOnlyList<BattleObject> GetValidTargets(Ability ability)
	{
		return BattleActionValidator.GetValidTargets(BattleContext, TurnContext, ability);
	}

	public IReadOnlyList<Vector3Int> GetValidTargetCells(Ability ability)
	{
		return BattleActionValidator.GetValidTargetCells(BattleContext, TurnContext, ability);
	}

	public IReadOnlyList<Vector3Int> GetAffectedCells(Ability ability, Vector3Int targetCell)
	{
		return !CanCastAtCell(ability, targetCell)
			? System.Array.Empty<Vector3Int>()
			: BattleTargetingRules.GetAffectedCells(BattleContext, ability, targetCell);
	}

	public IReadOnlyList<BattleObject> GetAffectedObjects(Ability ability, Vector3Int targetCell)
	{
		return !CanCastAtCell(ability, targetCell)
			? System.Array.Empty<BattleObject>()
			: BattleTargetingRules.GetAffectedObjects(BattleContext, ability, targetCell);
	}

	public IReadOnlyList<Vector3Int> GetMovementRangeMaskCells()
	{
		return BattleMaskRules.GetMovementRangeCells(BattleContext, TurnContext);
	}

	public IReadOnlyList<Vector3Int> GetAttackRangeMaskCells(Ability ability)
	{
		return BattleMaskRules.GetAttackRangeCells(BattleContext, TurnContext?.ActiveUnit, ability);
	}

	public IReadOnlyList<Vector3Int> GetAttackRangeMaskCells(Vector3Int sourceCell, Ability.RangeDefinition range, int bonusRange = 0)
	{
		return BattleMaskRules.GetAttackRangeCells(BattleContext, sourceCell, range, bonusRange);
	}

	public IReadOnlyList<Vector3Int> GetAreaOfEffectMaskCells(Ability ability, Vector3Int targetCell)
	{
		return !CanCastAtCell(ability, targetCell)
			? System.Array.Empty<Vector3Int>()
			: BattleMaskRules.GetAreaOfEffectCells(BattleContext, ability, targetCell);
	}

	public bool CanTarget(Ability ability, BattleObject target)
	{
		return BattleActionValidator.CanTarget(BattleContext, TurnContext, ability, target);
	}

	public bool CanTargetCell(Ability ability, Vector3Int cell)
	{
		return BattleActionValidator.CanTargetCell(BattleContext, TurnContext, ability, cell);
	}

	public bool CanCastAtCell(Ability ability, Vector3Int cell)
	{
		return BattleActionValidator.CanCastAtCell(BattleContext, TurnContext, ability, cell);
	}

	public bool CanEndTurn()
	{
		return BattleActionValidator.CanEndTurn(BattleContext, TurnContext);
	}

	public bool TrySubmitMove(Vector3Int destination)
	{
		if (TurnContext?.ActiveUnit == null || !CanMoveTo(destination))
		{
			return false;
		}

		return Orchestrator.TrySubmitPendingAction(new MoveAction(TurnContext.ActiveUnit, destination));
	}

	public bool TrySubmitAbility(Ability ability, IReadOnlyList<Vector3Int> targetCells)
	{
		if (TurnContext?.ActiveUnit == null || ability == null || !CanUseAbility(ability))
		{
			return false;
		}

		if (targetCells == null || targetCells.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < targetCells.Count; index++)
		{
			if (!CanCastAtCell(ability, targetCells[index]))
			{
				return false;
			}
		}

		return Orchestrator.TrySubmitPendingAction(new AbilityAction(TurnContext.ActiveUnit, ability, targetCells));
	}

	public bool TryGetPathTo(Vector3Int destination, out IReadOnlyList<Vector3Int> path)
	{
		return BattleActionValidator.TryGetPathTo(BattleContext, TurnContext, destination, out path);
	}

	public bool TrySubmitEndTurn()
	{
		if (!CanEndTurn())
		{
			return false;
		}

		return Orchestrator.TrySubmitPendingAction(new EndTurnAction(TurnContext.ActiveUnit));
	}
}
