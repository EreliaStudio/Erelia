using System.Collections.Generic;
using UnityEngine;

// Hook execution semantics:
// Order: hooks fire in status list order (lowest index first). No priority system.
// Cascades: effects applied during a hook do not trigger additional hooks. ApplyHook is not reentrant.
// Defeat during chain: if a resource rule defeats a unit mid-hook chain, remaining effects still fire.
//   The unit lifecycle transition is resolved by BattleUnitRules when HP crosses to zero.
// Source passives: source passives participate in hook evaluation normally.
//   Use BattleStatuses.Contains(status, includeSourcePassives: false) in cleanse effects that must skip them.
public static class BattleStatusRules
{
	private static bool isApplyingHook;

	public static void ApplyHook(BattleHookContext hookContext)
	{
		BattleUnit unit = hookContext?.HookOwner;
		BattleContext battleContext = hookContext?.BattleContext;
		StatusHookPoint hookPoint = hookContext?.HookPoint ?? default;
		if (unit == null || battleContext == null || isApplyingHook)
		{
			return;
		}

		isApplyingHook = true;
		try
		{
			List<Status> triggeredStatuses = new List<Status>();
			for (int index = 0; index < unit.Statuses.Count; index++)
			{
				BattleStatus battleStatus = unit.Statuses[index]?.Value;
				if (battleStatus?.Status == null || battleStatus.Status.HookPoint != hookPoint)
				{
					continue;
				}

				triggeredStatuses.Add(battleStatus.Status);
			}

			for (int index = 0; index < triggeredStatuses.Count; index++)
			{
				Status status = triggeredStatuses[index];
				if (status?.Effects == null)
				{
					continue;
				}

				bool anchorSet = false;
				Vector3Int anchorCell = default;
				Vector3Int affectedCell = default;
				if (battleContext?.Board?.Runtime != null)
				{
					if (hookContext.SourceObject != null && battleContext.Board.Runtime.TryGetPosition(hookContext.SourceObject, out Vector3Int casterPosition))
					{
						anchorCell = casterPosition;
						anchorSet = true;
					}

					BattleObject targetObject = hookContext.TargetObject ?? unit;
					if (battleContext.Board.Runtime.TryGetPosition(targetObject, out Vector3Int unitPosition))
					{
						affectedCell = unitPosition;
						if (!anchorSet)
						{
							anchorCell = unitPosition;
						}
					}
				}

				var context = new BattleAbilityExecutionContext
				{
					BattleContext = battleContext,
					Ability = null,
					SourceObject = hookContext.SourceObject ?? unit,
					TargetObject = hookContext.TargetObject ?? unit,
					AnchorCell = anchorCell,
					AffectedCell = affectedCell
				};

				for (int effectIndex = 0; effectIndex < status.Effects.Count; effectIndex++)
				{
					status.Effects[effectIndex]?.Apply(context);
				}
			}
		}
		finally
		{
			isApplyingHook = false;
		}
	}

	public static void AdvanceTurnDurations(BattleUnit unit)
	{
		unit?.Statuses.AdvanceTurnDurations();
	}
}
