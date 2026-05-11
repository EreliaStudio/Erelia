using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerTurnPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.PlayerTurn;
	private BattleActionCompositionService ActionCompositionService => ServiceLocator.Instance?.BattleActionCompositionService;

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
		if (!CanTargetCellWithAbility(ability, targetCell))
		{
			return System.Array.Empty<Vector3Int>();
		}

		Vector3Int? casterCell = TurnContext?.ActiveUnit?.HasBoardPosition == true ? TurnContext.ActiveUnit.BoardPosition : (Vector3Int?)null;
		return BattleTargetingRules.GetAffectedCells(BattleContext, ability, targetCell, casterCell);
	}

	public IReadOnlyList<BattleObject> GetAffectedObjects(Ability ability, Vector3Int targetCell)
	{
		if (!CanTargetCellWithAbility(ability, targetCell))
		{
			return System.Array.Empty<BattleObject>();
		}

		Vector3Int? casterCell = TurnContext?.ActiveUnit?.HasBoardPosition == true ? TurnContext.ActiveUnit.BoardPosition : (Vector3Int?)null;
		return BattleTargetingRules.GetAffectedObjects(BattleContext, ability, targetCell, casterCell);
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
		return !CanTargetCellWithAbility(ability, targetCell)
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

	// Cell-only check: range, LOS, board bounds, target profile. Does not check caster resources.
	public bool CanTargetCellWithAbility(Ability ability, Vector3Int cell)
	{
		return BattleActionValidator.CanTargetCellWithAbility(BattleContext, TurnContext, ability, cell);
	}

	// Combined check: caster resources + cell validity.
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
		return ActionCompositionService?.TryComposeMovement(destination) ?? false;
	}

	public bool TrySelectAbility(Ability ability)
	{
		return ActionCompositionService?.TrySelectAbility(ability) ?? false;
	}

	public bool TrySubmitSelectedAbilityTarget(Vector3Int targetCell)
	{
		return ActionCompositionService?.TryComposeAbilityTarget(targetCell) ?? false;
	}

	public bool TrySubmitAbility(Ability ability, IReadOnlyList<Vector3Int> targetCells)
	{
		return ActionCompositionService != null &&
			ActionCompositionService.TrySelectAbility(ability) &&
			ActionCompositionService.TryComposeAbilityTargets(targetCells);
	}

	public bool TryGetPathTo(Vector3Int destination, out IReadOnlyList<Vector3Int> path)
	{
		return BattleActionValidator.TryGetPathTo(BattleContext, TurnContext, destination, out path);
	}

	public bool TrySubmitEndTurn()
	{
		return ActionCompositionService?.TryComposeEndTurn() ?? false;
	}
}
