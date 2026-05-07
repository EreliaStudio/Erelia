using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class FeatRequirement
{
	public enum Scope
	{
		Action,
		Turn,
		Fight,
		Game
	}

	[Serializable]
	public struct Advancement
	{
		public float Progress;
		public int CompletedRepeatCount;

		public Advancement(float p_progress, int p_completedRepeatCount)
		{
			Progress = p_progress;
			CompletedRepeatCount = p_completedRepeatCount;
		}
	}

	public Scope RequirementScope = Scope.Fight;
	public int RequiredRepeatCount = 1;

	public virtual Advancement EvaluateEvents(
		IReadOnlyList<BattleEvent> p_events,
		Advancement p_currentAdvancement)
	{
		Advancement advancement = p_currentAdvancement;

		if (IsCompleted(advancement))
		{
			return advancement;
		}

		if (p_events == null || p_events.Count == 0)
		{
			return advancement;
		}

		switch (RequirementScope)
		{
			case Scope.Action:
				EvaluateActionScope(p_events, ref advancement);
				break;

			case Scope.Turn:
				EvaluateTurnScope(p_events, ref advancement);
				break;

			case Scope.Fight:
				EvaluateFightScope(p_events, ref advancement);
				break;

			case Scope.Game:
				EvaluateGameScope(p_events, ref advancement);
				break;
		}

		return advancement;
	}

	public bool IsCompleted(Advancement p_advancement)
	{
		return p_advancement.CompletedRepeatCount >= RequiredRepeatCount;
	}

	protected abstract float EvaluateEventProgress(BattleEvent p_event);

	protected static float ComputeLinearProgress(int p_amount, int p_requiredAmount)
	{
		if (p_requiredAmount <= 0)
		{
			return 100f;
		}

		if (p_amount <= 0)
		{
			return 0f;
		}

		return (float)p_amount / p_requiredAmount * 100f;
	}

	private void EvaluateActionScope(
		IReadOnlyList<BattleEvent> p_events,
		ref Advancement p_advancement)
	{
		for (int index = 0; index < p_events.Count; index++)
		{
			float eventProgress = GetEventProgress(p_events[index]);

			if (eventProgress < 100f)
			{
				continue;
			}

			RegisterOneCompletion(ref p_advancement);

			if (IsCompleted(p_advancement))
			{
				return;
			}
		}
	}

	private void EvaluateTurnScope(
		IReadOnlyList<BattleEvent> p_events,
		ref Advancement p_advancement)
	{
		Dictionary<int, float> progressByTurn = new Dictionary<int, float>();

		for (int index = 0; index < p_events.Count; index++)
		{
			BattleEvent battleEvent = p_events[index];

			if (battleEvent == null)
			{
				continue;
			}

			float eventProgress = GetEventProgress(battleEvent);

			if (eventProgress <= 0f)
			{
				continue;
			}

			if (progressByTurn.ContainsKey(battleEvent.TurnIndex) == false)
			{
				progressByTurn.Add(battleEvent.TurnIndex, 0f);
			}

			progressByTurn[battleEvent.TurnIndex] += eventProgress;
		}

		foreach (float turnProgress in progressByTurn.Values)
		{
			if (turnProgress < 100f)
			{
				continue;
			}

			RegisterOneCompletion(ref p_advancement);

			if (IsCompleted(p_advancement))
			{
				return;
			}
		}
	}

	private void EvaluateFightScope(
		IReadOnlyList<BattleEvent> p_events,
		ref Advancement p_advancement)
	{
		float fightProgress = p_advancement.Progress;

		for (int index = 0; index < p_events.Count; index++)
		{
			fightProgress += GetEventProgress(p_events[index]);

			if (fightProgress >= 100f)
			{
				RegisterOneCompletion(ref p_advancement);

				if (IsCompleted(p_advancement))
				{
					return;
				}

				fightProgress = 0f;
			}
		}

		p_advancement.Progress = fightProgress;
	}

	private void EvaluateGameScope(
		IReadOnlyList<BattleEvent> p_events,
		ref Advancement p_advancement)
	{
		for (int index = 0; index < p_events.Count; index++)
		{
			p_advancement.Progress += GetEventProgress(p_events[index]);

			if (p_advancement.Progress < 100f)
			{
				continue;
			}

			RegisterOneCompletion(ref p_advancement);

			if (IsCompleted(p_advancement))
			{
				return;
			}
		}
	}

	private float GetEventProgress(BattleEvent p_event)
	{
		if (p_event == null)
		{
			return 0f;
		}

		return Mathf.Max(0f, EvaluateEventProgress(p_event));
	}

	private void RegisterOneCompletion(ref Advancement p_advancement)
	{
		p_advancement.CompletedRepeatCount++;
		p_advancement.Progress = 0f;
	}
}

[Serializable]
public abstract class FeatRequirementTemplated<TEvent> : FeatRequirement
	where TEvent : BattleEvent
{
	protected sealed override float EvaluateEventProgress(BattleEvent p_event)
	{
		if (p_event is not TEvent typedEvent)
		{
			return 0f;
		}

		return EvaluateProgress(typedEvent);
	}

	protected abstract float EvaluateProgress(TEvent p_event);
}

// ── Damage ────────────────────────────────────────────────────────────────────

[Serializable]
public class DealDamageRequirement : FeatRequirementTemplated<DamageEvent>
{
	public int RequiredAmount = 10;
	public List<Ability> SourceAbilities = new();
	public bool FilterByDamageKind = false;
	public MathFormula.DamageInput.Kind RequiredDamageKind = MathFormula.DamageInput.Kind.Physical;

	protected override float EvaluateProgress(DamageEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.SourceAbility)) return 0f;
		if (FilterByDamageKind && p_event.DamageKind != RequiredDamageKind) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class TakeDamageRequirement : FeatRequirementTemplated<DamageEvent>
{
	public int RequiredAmount = 10;

	protected override float EvaluateProgress(DamageEvent p_event)
	{
		if (p_event.Target == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class WinAfterDealingDamageRequirement : FeatRequirementTemplated<DamageEvent>
{
	public int RequiredAmount = 100;

	public WinAfterDealingDamageRequirement()
	{
		RequirementScope = Scope.Fight;
	}

	protected override float EvaluateProgress(DamageEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class SurviveHitRequirement : FeatRequirementTemplated<HitSurvivedEvent>
{
	public int RequiredAmount = 10;

	public SurviveHitRequirement()
	{
		RequirementScope = Scope.Action;
	}

	protected override float EvaluateProgress(HitSurvivedEvent p_event)
	{
		return p_event.Amount >= RequiredAmount ? 100f : 0f;
	}
}

// ── Healing ───────────────────────────────────────────────────────────────────

[Serializable]
public class HealHealthRequirement : FeatRequirementTemplated<HealEvent>
{
	public int RequiredAmount = 10;

	protected override float EvaluateProgress(HealEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class HealTargetRequirement : FeatRequirementTemplated<HealEvent>
{
	public enum TargetFilter { Self, Ally, Any }

	public int RequiredAmount = 10;
	public TargetFilter Target = TargetFilter.Any;

	protected override float EvaluateProgress(HealEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		bool isSelf = p_event.Caster == p_event.Target;
		if (Target == TargetFilter.Self && !isSelf) return 0f;
		if (Target == TargetFilter.Ally && isSelf) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class WinAfterHealingRequirement : FeatRequirementTemplated<HealEvent>
{
	public int RequiredAmount = 50;

	public WinAfterHealingRequirement()
	{
		RequirementScope = Scope.Fight;
	}

	protected override float EvaluateProgress(HealEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

// ── Shields ───────────────────────────────────────────────────────────────────

[Serializable]
public class ApplyShieldRequirement : FeatRequirementTemplated<ShieldAppliedEvent>
{
	public enum KindFilter { Any, Physical, Magical }

	public int RequiredAmount = 10;
	public KindFilter Filter = KindFilter.Any;

	protected override float EvaluateProgress(ShieldAppliedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (Filter == KindFilter.Physical && p_event.Kind != ShieldKind.Physical) return 0f;
		if (Filter == KindFilter.Magical && p_event.Kind != ShieldKind.Magical) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class ApplyShieldCountRequirement : FeatRequirementTemplated<ShieldAppliedEvent>
{
	public int RequiredCount = 3;

	protected override float EvaluateProgress(ShieldAppliedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class AbsorbDamageWithShieldRequirement : FeatRequirementTemplated<DamageAbsorbedEvent>
{
	public int RequiredAmount = 10;

	protected override float EvaluateProgress(DamageAbsorbedEvent p_event)
	{
		if (p_event.Target == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class MaxDamageAbsorbedInOneHitRequirement : FeatRequirementTemplated<DamageAbsorbedEvent>
{
	public int RequiredAmount = 10;

	public MaxDamageAbsorbedInOneHitRequirement()
	{
		RequirementScope = Scope.Action;
	}

	protected override float EvaluateProgress(DamageAbsorbedEvent p_event)
	{
		if (p_event.Target == null) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class ShieldBrokenRequirement : FeatRequirementTemplated<ShieldBrokenEvent>
{
	public enum KindFilter { Any, Physical, Magical }

	public int RequiredCount = 1;
	public KindFilter Filter = KindFilter.Any;

	protected override float EvaluateProgress(ShieldBrokenEvent p_event)
	{
		if (Filter == KindFilter.Physical && p_event.Kind != ShieldKind.Physical) return 0f;
		if (Filter == KindFilter.Magical && p_event.Kind != ShieldKind.Magical) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

// ── Status ────────────────────────────────────────────────────────────────────

[Serializable]
public class ApplyStatusCountRequirement : FeatRequirementTemplated<StatusAppliedEvent>
{
	public int RequiredCount = 5;
	public Status RequiredStatus;
	public List<Ability> SourceAbilities = new();

	protected override float EvaluateProgress(StatusAppliedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (RequiredStatus != null && p_event.Status != RequiredStatus) return 0f;
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.SourceAbility)) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class RemoveStatusCountRequirement : FeatRequirementTemplated<StatusRemovedEvent>
{
	public int RequiredCount = 1;
	public Status RequiredStatus;

	protected override float EvaluateProgress(StatusRemovedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (RequiredStatus != null && p_event.Status != RequiredStatus) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

// ── Kills ─────────────────────────────────────────────────────────────────────

[Serializable]
public class KillCountRequirement : FeatRequirementTemplated<UnitDefeatedEvent>
{
	public int RequiredCount = 3;
	public List<Ability> SourceAbilities = new();

	protected override float EvaluateProgress(UnitDefeatedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.SourceAbility)) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class LastHitRequirement : FeatRequirementTemplated<UnitDefeatedEvent>
{
	public int RequiredCount = 5;

	protected override float EvaluateProgress(UnitDefeatedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

// ── Battle outcome ────────────────────────────────────────────────────────────

[Serializable]
public class WinBattleCountRequirement : FeatRequirementTemplated<BattleWonEvent>
{
	public bool RequireUnitSurvival = false;

	public WinBattleCountRequirement()
	{
		RequirementScope = Scope.Game;
	}

	protected override float EvaluateProgress(BattleWonEvent p_event)
	{
		if (RequireUnitSurvival && !p_event.UnitSurvived) return 0f;
		return 100f;
	}
}

// ── Resources ─────────────────────────────────────────────────────────────────

[Serializable]
public class ConsumeResourcesRequirement : FeatRequirementTemplated<ResourceConsumedEvent>
{
	public ResourceConsumedEvent.ResourceKind RequiredResource = ResourceConsumedEvent.ResourceKind.ActionPoints;
	public int RequiredAmount = 10;

	protected override float EvaluateProgress(ResourceConsumedEvent p_event)
	{
		if (p_event.Resource != RequiredResource) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

// ── Movement ──────────────────────────────────────────────────────────────────

[Serializable]
public class TotalDistanceTravelledRequirement : FeatRequirementTemplated<DistanceTravelledEvent>
{
	public int RequiredDistance = 10;

	protected override float EvaluateProgress(DistanceTravelledEvent p_event)
	{
		return ComputeLinearProgress(p_event.Distance, RequiredDistance);
	}
}

// Tracks distance pushed/pulled to or from targets.
// Use on the caster's feat (Caster != null) for "push enemies X squares total".
[Serializable]
public class DisplacementDealtRequirement : FeatRequirementTemplated<DisplacementEvent>
{
	public int RequiredDistance = 5;
	public bool FilterByOrientation = false;
	public MoveStatus.Orientation RequiredOrientation = MoveStatus.Orientation.AwayFromCaster;

	protected override float EvaluateProgress(DisplacementEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		if (FilterByOrientation && p_event.Orientation != RequiredOrientation) return 0f;
		return ComputeLinearProgress(p_event.Distance, RequiredDistance);
	}
}

// Tracks being displaced (pushed or pulled) by any source.
// Use on the target's feat (Target != null) for "be pushed X times".
[Serializable]
public class DisplacementReceivedRequirement : FeatRequirementTemplated<DisplacementEvent>
{
	public int RequiredCount = 3;
	public bool FilterByOrientation = false;
	public MoveStatus.Orientation RequiredOrientation = MoveStatus.Orientation.AwayFromCaster;

	protected override float EvaluateProgress(DisplacementEvent p_event)
	{
		if (p_event.Target == null) return 0f;
		if (FilterByOrientation && p_event.Orientation != RequiredOrientation) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

// ── Teleport ──────────────────────────────────────────────────────────────────

[Serializable]
public class TeleportCountRequirement : FeatRequirementTemplated<TeleportedEvent>
{
	public int RequiredCount = 1;

	protected override float EvaluateProgress(TeleportedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class TeleportDistanceRequirement : FeatRequirementTemplated<TeleportedEvent>
{
	public int RequiredDistance = 10;

	protected override float EvaluateProgress(TeleportedEvent p_event)
	{
		if (p_event.Caster == null) return 0f;
		return ComputeLinearProgress(p_event.Distance, RequiredDistance);
	}
}

// ── Positional ────────────────────────────────────────────────────────────────

[Serializable]
public class TurnStartPositionRequirement : FeatRequirementTemplated<TurnStartedEvent>
{
	public enum TargetKind { Ally, Enemy, AnyUnit }
	public enum DistanceKind { Within, AtLeast, Between }

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;
	public int MaximumDistance = 3;

	public TurnStartPositionRequirement()
	{
		RequirementScope = Scope.Action;
	}

	protected override float EvaluateProgress(TurnStartedEvent p_event)
	{
		int closest = ResolveDistance(p_event);
		return MeetsCondition(closest) ? 100f : 0f;
	}

	private int ResolveDistance(TurnStartedEvent p_event)
	{
		return Target switch
		{
			TargetKind.Ally => p_event.ClosestAllyDistance,
			TargetKind.Enemy => p_event.ClosestEnemyDistance,
			TargetKind.AnyUnit => Math.Min(p_event.ClosestAllyDistance, p_event.ClosestEnemyDistance),
			_ => int.MaxValue
		};
	}

	private bool MeetsCondition(int closest)
	{
		return Condition switch
		{
			DistanceKind.Within => closest <= Distance,
			DistanceKind.AtLeast => closest >= Distance,
			DistanceKind.Between => closest >= Math.Min(Distance, MaximumDistance) &&
				closest <= Math.Max(Distance, MaximumDistance),
			_ => false
		};
	}
}

[Serializable]
public class TurnEndPositionRequirement : FeatRequirementTemplated<TurnEndedEvent>
{
	public enum TargetKind { Ally, Enemy, AnyUnit }
	public enum DistanceKind { Within, AtLeast, Between }

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;
	public int MaximumDistance = 3;

	public TurnEndPositionRequirement()
	{
		RequirementScope = Scope.Action;
	}

	protected override float EvaluateProgress(TurnEndedEvent p_event)
	{
		int closest = ResolveDistance(p_event);
		return MeetsCondition(closest) ? 100f : 0f;
	}

	private int ResolveDistance(TurnEndedEvent p_event)
	{
		return Target switch
		{
			TargetKind.Ally => p_event.ClosestAllyDistance,
			TargetKind.Enemy => p_event.ClosestEnemyDistance,
			TargetKind.AnyUnit => Math.Min(p_event.ClosestAllyDistance, p_event.ClosestEnemyDistance),
			_ => int.MaxValue
		};
	}

	private bool MeetsCondition(int closest)
	{
		return Condition switch
		{
			DistanceKind.Within => closest <= Distance,
			DistanceKind.AtLeast => closest >= Distance,
			DistanceKind.Between => closest >= Math.Min(Distance, MaximumDistance) &&
				closest <= Math.Max(Distance, MaximumDistance),
			_ => false
		};
	}
}

// ── Ability casting ───────────────────────────────────────────────────────────

[Serializable]
public class CastAbilityCountRequirement : FeatRequirementTemplated<AbilityCastEvent>
{
	public List<Ability> Abilities = new();
	public int RequiredCount = 1;

	protected override float EvaluateProgress(AbilityCastEvent p_event)
	{
		if (Abilities.Count > 0 && !Abilities.Contains(p_event.SourceAbility))
		{
			return 0f;
		}

		return ComputeLinearProgress(1, RequiredCount);
	}
}

// ── Meta ──────────────────────────────────────────────────────────────────────

[Serializable]
public class AndRequirement : FeatRequirement
{
	public List<FeatRequirement> Children = new();

	public override Advancement EvaluateEvents(IReadOnlyList<BattleEvent> p_events, Advancement p_currentAdvancement)
	{
		if (Children == null || Children.Count == 0 || IsCompleted(p_currentAdvancement))
			return p_currentAdvancement;

		float minProgress = 100f;
		bool allComplete = true;

		for (int i = 0; i < Children.Count; i++)
		{
			if (Children[i] == null) continue;
			Advancement childAdv = Children[i].EvaluateEvents(p_events, default);
			bool childDone = Children[i].IsCompleted(childAdv);
			allComplete &= childDone;
			float childPct = childDone ? 100f : childAdv.Progress;
			if (childPct < minProgress) minProgress = childPct;
		}

		Advancement advancement = p_currentAdvancement;
		if (allComplete)
		{
			advancement.CompletedRepeatCount++;
			advancement.Progress = 0f;
		}
		else
		{
			advancement.Progress = minProgress;
		}

		return advancement;
	}

	protected override float EvaluateEventProgress(BattleEvent p_event) => 0f;
}

[Serializable]
public class OrRequirement : FeatRequirement
{
	public List<FeatRequirement> Children = new();

	public override Advancement EvaluateEvents(IReadOnlyList<BattleEvent> p_events, Advancement p_currentAdvancement)
	{
		if (Children == null || Children.Count == 0 || IsCompleted(p_currentAdvancement))
			return p_currentAdvancement;

		float maxProgress = 0f;
		bool anyComplete = false;

		for (int i = 0; i < Children.Count; i++)
		{
			if (Children[i] == null) continue;
			Advancement childAdv = Children[i].EvaluateEvents(p_events, default);
			bool childDone = Children[i].IsCompleted(childAdv);
			if (childDone) anyComplete = true;
			float childPct = childDone ? 100f : childAdv.Progress;
			if (childPct > maxProgress) maxProgress = childPct;
		}

		Advancement advancement = p_currentAdvancement;
		if (anyComplete)
		{
			advancement.CompletedRepeatCount++;
			advancement.Progress = 0f;
		}
		else
		{
			advancement.Progress = maxProgress;
		}

		return advancement;
	}

	protected override float EvaluateEventProgress(BattleEvent p_event) => 0f;
}
