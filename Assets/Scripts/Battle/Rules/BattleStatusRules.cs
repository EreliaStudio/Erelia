using System.Collections.Generic;
using UnityEngine;

// Hook execution semantics:
// Order: hooks fire in status list order (lowest index first). No priority system.
// Cascades: effects applied during a hook do not trigger additional hooks. ApplyHook is not reentrant.
// Defeat during chain: if a unit is defeated mid-hook chain, remaining effects still fire.
//   Defeat is resolved by the resolver after the full action completes, not mid-hook.
// Source passives: source passives participate in hook evaluation normally.
//   Use BattleStatuses.Contains(status, includeSourcePassives: false) in cleanse effects that must skip them.
public static class BattleStatusRules
{
	public static void ApplyHook(BattleUnit unit, BattleContext battleContext, StatusHookPoint hookPoint, BattleObject caster = null)
	{
		if (unit == null || battleContext == null)
		{
			return;
		}

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
				if (caster != null && battleContext.Board.Runtime.TryGetPosition(caster, out Vector3Int casterPosition))
				{
					anchorCell = casterPosition;
					anchorSet = true;
				}

				if (battleContext.Board.Runtime.TryGetPosition(unit, out Vector3Int unitPosition))
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
				SourceObject = caster ?? unit,
				TargetObject = unit,
				AnchorCell = anchorCell,
				AffectedCell = affectedCell
			};

			for (int effectIndex = 0; effectIndex < status.Effects.Count; effectIndex++)
			{
				status.Effects[effectIndex]?.Apply(context);
			}
		}
	}

	public static void AdvanceTurnDurations(BattleUnit unit)
	{
		unit?.Statuses.AdvanceTurnDurations();
	}
}
