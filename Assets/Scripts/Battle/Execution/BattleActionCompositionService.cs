using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleActionCompositionContext
{
	public BattleContext BattleContext { get; set; }
	public BattleAction Action { get; set; }

	public BattleActionCompositionContext Clone()
	{
		return new BattleActionCompositionContext
		{
			BattleContext = BattleContext,
			Action = Action
		};
	}
}

public sealed class BattleActionCompositionService
{
	private BattleContext battleContext;
	private BattleActionCompositionContext pendingContext;

	public BattleActionCompositionContext PendingContext => pendingContext;
	public bool HasPendingContext => pendingContext != null;

	public void Initialize()
	{
		EventCenter.BattleStarted += OnBattleStarted;
		EventCenter.BattleResolved += OnBattleResolved;
	}

	public void Shutdown()
	{
		EventCenter.BattleStarted -= OnBattleStarted;
		EventCenter.BattleResolved -= OnBattleResolved;
		Clear();
	}

	public void Bind(BattleContext p_battleContext)
	{
		CancelPending();
		battleContext = p_battleContext;
	}

	public void Clear()
	{
		CancelPending();
		battleContext = null;
	}

	public bool TryBeginMovementSelection()
	{
		return TryGetActiveUnit(out _);
	}

	public bool TryComposeMovement(Vector3Int destination)
	{
		if (!TryGetActiveUnit(out BattleUnit activeUnit))
		{
			return false;
		}

		if (!BattleActionValidator.TryGetMovementCost(battleContext, battleContext.CurrentTurn, destination, out int movementCost))
		{
			return false;
		}

		pendingContext = new BattleActionCompositionContext
		{
			BattleContext = battleContext,
			Action = new MoveAction(activeUnit, destination, movementCost)
		};

		EventCenter.EmitBattleActionCompositionStarted(pendingContext.Clone());
		EventCenter.EmitBattleActionCompositionUpdated(pendingContext.Clone());
		return TryConfirmPendingAction();
	}

	public bool TrySelectAbility(Ability ability)
	{
		if (!TryGetActiveUnit(out BattleUnit activeUnit))
		{
			return false;
		}

		if (ability == null)
		{
			return false;
		}

		if (!DoesUnitOwnAbility(activeUnit, ability))
		{
			return false;
		}

		if (!BattleActionValidator.CanUseAbility(battleContext, battleContext.CurrentTurn, ability))
		{
			return false;
		}

		pendingContext = new BattleActionCompositionContext
		{
			BattleContext = battleContext,
			Action = new AbilityAction(activeUnit, ability, Array.Empty<Vector3Int>())
		};

		EventCenter.EmitBattleActionCompositionStarted(pendingContext.Clone());
		return true;
	}

	public bool TrySelectAbilityTarget(Vector3Int targetCell)
	{
		return TrySelectAbilityTargets(new[] { targetCell });
	}

	public bool TrySelectAbilityTargets(IReadOnlyList<Vector3Int> targetCells)
	{
		if (pendingContext?.Action is not AbilityAction abilityAction)
		{
			return false;
		}

		if (targetCells == null || targetCells.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < targetCells.Count; index++)
		{
			Vector3Int targetCell = targetCells[index];
			AbilityCastLegality targetLegality = BattleActionValidator.GetCastLegality(
				battleContext,
				battleContext.CurrentTurn,
				abilityAction.Ability,
				targetCell);

			if (!targetLegality.IsValid)
			{
				return false;
			}
		}

		pendingContext.Action = new AbilityAction(abilityAction.SourceUnit, abilityAction.Ability, new List<Vector3Int>(targetCells));
		EventCenter.EmitBattleActionCompositionUpdated(pendingContext.Clone());
		return true;
	}

	public bool TryComposeAbilityTarget(Vector3Int targetCell)
	{
		return TrySelectAbilityTarget(targetCell) && TryConfirmPendingAction();
	}

	public bool TryComposeAbilityTargets(IReadOnlyList<Vector3Int> targetCells)
	{
		return TrySelectAbilityTargets(targetCells) && TryConfirmPendingAction();
	}

	public bool TryComposeEndTurn()
	{
		if (!TryGetActiveUnit(out BattleUnit activeUnit) ||
			!BattleActionValidator.CanEndTurn(battleContext, battleContext.CurrentTurn))
		{
			return false;
		}

		pendingContext = new BattleActionCompositionContext
		{
			BattleContext = battleContext,
			Action = new EndTurnAction(activeUnit)
		};

		EventCenter.EmitBattleActionCompositionStarted(pendingContext.Clone());
		EventCenter.EmitBattleActionCompositionUpdated(pendingContext.Clone());
		return TryConfirmPendingAction();
	}

	public bool TryConfirmPendingAction()
	{
		if (pendingContext == null)
		{
			return false;
		}

		if (!IsActionComplete(pendingContext.Action))
		{
			return false;
		}

		BattleActionCompositionContext completedContext = pendingContext.Clone();
		BattleAction action = pendingContext.Action;
		pendingContext = null;
		EventCenter.EmitBattleActionComposed(completedContext, action);
		return true;
	}

	public void CancelPending()
	{
		if (pendingContext == null)
		{
			return;
		}

		BattleActionCompositionContext canceledContext = pendingContext.Clone();
		pendingContext = null;
		EventCenter.EmitBattleActionCompositionCanceled(canceledContext);
	}

	private bool TryGetActiveUnit(out BattleUnit activeUnit)
	{
		activeUnit = null;

		TurnContext turnContext = battleContext?.CurrentTurn;
		if (battleContext == null || turnContext == null)
		{
			return false;
		}

		if (turnContext.HasPendingAction)
		{
			return false;
		}

		activeUnit = turnContext.ActiveUnit;
		if (activeUnit == null)
		{
			return false;
		}

		if (activeUnit.IsDefeated)
		{
			return false;
		}

		if (!activeUnit.HasBoardPosition)
		{
			return false;
		}

		return true;
	}

	private static bool IsActionComplete(BattleAction action)
	{
		return action switch
		{
			MoveAction moveAction => moveAction.MovementPointCost > 0,
			AbilityAction abilityAction => abilityAction.Ability != null &&
				abilityAction.TargetCells != null &&
				abilityAction.TargetCells.Count > 0,
			EndTurnAction => true,
			_ => false
		};
	}

	private static bool DoesUnitOwnAbility(BattleUnit unit, Ability ability)
	{
		if (unit?.Abilities == null || ability == null)
		{
			return false;
		}

		for (int index = 0; index < unit.Abilities.Count; index++)
		{
			if (unit.Abilities[index] == ability)
			{
				return true;
			}
		}

		return false;
	}

	private void OnBattleStarted(BattleContext p_battleContext)
	{
		Bind(p_battleContext);
	}

	private void OnBattleResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		if (ReferenceEquals(battleContext, p_battleContext))
		{
			Clear();
		}
	}
}
