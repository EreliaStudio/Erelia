using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class FeatRequirement
{
	public enum Scope
	{
		Ability,
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

	[Serializable]
	public abstract class EventBase
	{
		public int TurnIndex = 0;
		public Ability SourceAbility;
	}

	public Scope RequirementScope = Scope.Fight;
	public int RequiredRepeatCount = 1;

	public virtual Advancement EvaluateEvents(
		IReadOnlyList<EventBase> p_events,
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
			case Scope.Ability:
				EvaluateAbilityScope(p_events, ref advancement);
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

	protected abstract float EvaluateEventProgress(EventBase p_event);

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

	private void EvaluateAbilityScope(
		IReadOnlyList<EventBase> p_events,
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
		IReadOnlyList<EventBase> p_events,
		ref Advancement p_advancement)
	{
		Dictionary<int, float> progressByTurn = new Dictionary<int, float>();

		for (int index = 0; index < p_events.Count; index++)
		{
			EventBase featEvent = p_events[index];

			if (featEvent == null)
			{
				continue;
			}

			float eventProgress = GetEventProgress(featEvent);

			if (eventProgress <= 0f)
			{
				continue;
			}

			if (progressByTurn.ContainsKey(featEvent.TurnIndex) == false)
			{
				progressByTurn.Add(featEvent.TurnIndex, 0f);
			}

			progressByTurn[featEvent.TurnIndex] += eventProgress;
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
		IReadOnlyList<EventBase> p_events,
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
		IReadOnlyList<EventBase> p_events,
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

	private float GetEventProgress(EventBase p_event)
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
	where TEvent : FeatRequirement.EventBase
{
	protected sealed override float EvaluateEventProgress(EventBase p_event)
	{
		if (p_event is not TEvent typedEvent)
		{
			return 0f;
		}

		return EvaluateProgress(typedEvent);
	}

	protected abstract float EvaluateProgress(TEvent p_event);
}

[Serializable]
public class DealDamageRequirement : FeatRequirementTemplated<DealDamageRequirement.Event>
{
	public int RequiredAmount = 10;
	public List<Ability> SourceAbilities = new();
	public bool FilterByDamageKind = false;
	public MathFormula.DamageInput.Kind RequiredDamageKind = MathFormula.DamageInput.Kind.Physical;

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
		public MathFormula.DamageInput.Kind DamageKind = MathFormula.DamageInput.Kind.Physical;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.SourceAbility)) return 0f;
		if (FilterByDamageKind && p_event.DamageKind != RequiredDamageKind) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class HealHealthRequirement : FeatRequirementTemplated<HealHealthRequirement.Event>
{
	public int RequiredAmount = 10;

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class CastAbilityCountRequirement : FeatRequirementTemplated<CastAbilityCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public Ability Ability;
	}

	public List<Ability> Abilities = new();
	public int RequiredCount = 1;

	protected override float EvaluateProgress(Event p_event)
	{
		if (Abilities.Count > 0 && !Abilities.Contains(p_event.Ability))
		{
			return 0f;
		}

		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class TakeDamageRequirement : FeatRequirementTemplated<TakeDamageRequirement.Event>
{
	public int RequiredAmount = 10;

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}


[Serializable]
public class TurnStartPositionRequirement : FeatRequirementTemplated<TurnStartPositionRequirement.Event>
{
	public enum TargetKind { Ally, Enemy, AnyUnit }
	public enum DistanceKind { Within, AtLeast, Between }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int ClosestAllyDistance = int.MaxValue;
		public int ClosestEnemyDistance = int.MaxValue;
	}

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;
	public int MaximumDistance = 3;

	public TurnStartPositionRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		int closest = ResolveDistance(p_event);
		return MeetsCondition(closest) ? 100f : 0f;
	}

	private int ResolveDistance(Event p_event)
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
public class TurnEndPositionRequirement : FeatRequirementTemplated<TurnEndPositionRequirement.Event>
{
	public enum TargetKind { Ally, Enemy, AnyUnit }
	public enum DistanceKind { Within, AtLeast, Between }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int ClosestAllyDistance = int.MaxValue;
		public int ClosestEnemyDistance = int.MaxValue;
	}

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;
	public int MaximumDistance = 3;

	public TurnEndPositionRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		int closest = ResolveDistance(p_event);
		return MeetsCondition(closest) ? 100f : 0f;
	}

	private int ResolveDistance(Event p_event)
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
public class ApplyShieldRequirement : FeatRequirementTemplated<ApplyShieldRequirement.Event>
{
	public enum KindFilter { Any, Physical, Magical }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
		public ShieldKind Kind = ShieldKind.Physical;
	}

	public int RequiredAmount = 10;
	public KindFilter Filter = KindFilter.Any;

	protected override float EvaluateProgress(Event p_event)
	{
		if (Filter == KindFilter.Physical && p_event.Kind != ShieldKind.Physical) return 0f;
		if (Filter == KindFilter.Magical && p_event.Kind != ShieldKind.Magical) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class AbsorbDamageWithShieldRequirement : FeatRequirementTemplated<AbsorbDamageWithShieldRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	public int RequiredAmount = 10;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class MaxDamageAbsorbedInOneHitRequirement : FeatRequirementTemplated<AbsorbDamageWithShieldRequirement.Event>
{
	public int RequiredAmount = 10;

	public MaxDamageAbsorbedInOneHitRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(AbsorbDamageWithShieldRequirement.Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class ShieldBrokenRequirement : FeatRequirementTemplated<ShieldBrokenRequirement.Event>
{
	public enum KindFilter { Any, Physical, Magical }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public ShieldKind Kind = ShieldKind.Physical;
	}

	public int RequiredCount = 1;
	public KindFilter Filter = KindFilter.Any;

	protected override float EvaluateProgress(Event p_event)
	{
		if (Filter == KindFilter.Physical && p_event.Kind != ShieldKind.Physical) return 0f;
		if (Filter == KindFilter.Magical && p_event.Kind != ShieldKind.Magical) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class WinAfterDealingDamageRequirement : FeatRequirementTemplated<DealDamageRequirement.Event>
{
	public int RequiredAmount = 100;

	public WinAfterDealingDamageRequirement()
	{
		RequirementScope = Scope.Fight;
	}

	protected override float EvaluateProgress(DealDamageRequirement.Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class SurviveHitRequirement : FeatRequirementTemplated<SurviveHitRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	public int RequiredAmount = 10;

	public SurviveHitRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		return p_event.Amount >= RequiredAmount ? 100f : 0f;
	}
}

[Serializable]
public class HealTargetRequirement : FeatRequirementTemplated<HealTargetRequirement.Event>
{
	public enum TargetFilter { Self, Ally, Any }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
		public bool IsSelf = false;
	}

	public int RequiredAmount = 10;
	public TargetFilter Target = TargetFilter.Any;

	protected override float EvaluateProgress(Event p_event)
	{
		if (Target == TargetFilter.Self && !p_event.IsSelf) return 0f;
		if (Target == TargetFilter.Ally && p_event.IsSelf) return 0f;
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class WinAfterHealingRequirement : FeatRequirementTemplated<HealHealthRequirement.Event>
{
	public int RequiredAmount = 50;

	public WinAfterHealingRequirement()
	{
		RequirementScope = Scope.Fight;
	}

	protected override float EvaluateProgress(HealHealthRequirement.Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class ApplyShieldCountRequirement : FeatRequirementTemplated<ApplyShieldCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase { }

	public int RequiredCount = 3;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class ApplyStatusCountRequirement : FeatRequirementTemplated<ApplyStatusCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public Status Status;
		public int StackCount = 1;
	}

	public int RequiredCount = 5;
	public Status RequiredStatus;
	public List<Ability> SourceAbilities = new();

	protected override float EvaluateProgress(Event p_event)
	{
		if (RequiredStatus != null && p_event.Status != RequiredStatus) return 0f;
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.SourceAbility)) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class KillCountRequirement : FeatRequirementTemplated<KillCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public Ability Ability;
	}

	public int RequiredCount = 3;
	public List<Ability> SourceAbilities = new();

	protected override float EvaluateProgress(Event p_event)
	{
		if (SourceAbilities.Count > 0 && !SourceAbilities.Contains(p_event.Ability)) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class LastHitRequirement : FeatRequirementTemplated<KillCountRequirement.Event>
{
	public int RequiredCount = 5;

	protected override float EvaluateProgress(KillCountRequirement.Event p_event)
	{
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class WinBattleCountRequirement : FeatRequirementTemplated<WinBattleCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public bool UnitSurvived = false;
	}

	public bool RequireUnitSurvival = false;

	public WinBattleCountRequirement()
	{
		RequirementScope = Scope.Game;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		if (RequireUnitSurvival && !p_event.UnitSurvived) return 0f;
		return 100f;
	}
}

[Serializable]
public class SpendActionPointsRequirement : FeatRequirementTemplated<SpendActionPointsRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	public int RequiredAmount = 10;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class SpendMovementPointsRequirement : FeatRequirementTemplated<SpendMovementPointsRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	public int RequiredAmount = 10;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class MoveCountRequirement : FeatRequirementTemplated<MoveCountRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase { }

	public int RequiredCount = 5;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(1, RequiredCount);
	}
}

[Serializable]
public class TotalDistanceTravelledRequirement : FeatRequirementTemplated<TotalDistanceTravelledRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Distance = 0;
	}

	public int RequiredDistance = 10;

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Distance, RequiredDistance);
	}
}

[Serializable]
public class MaxDistanceInOneMoveRequirement : FeatRequirementTemplated<MaxDistanceInOneMoveRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Distance = 0;
	}

	public int RequiredDistance = 4;

	public MaxDistanceInOneMoveRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Distance, RequiredDistance);
	}
}


[Serializable]
public class AndRequirement : FeatRequirement
{
	public List<FeatRequirement> Children = new();

	public override Advancement EvaluateEvents(IReadOnlyList<EventBase> p_events, Advancement p_currentAdvancement)
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

	protected override float EvaluateEventProgress(EventBase p_event) => 0f;
}

[Serializable]
public class OrRequirement : FeatRequirement
{
	public List<FeatRequirement> Children = new();

	public override Advancement EvaluateEvents(IReadOnlyList<EventBase> p_events, Advancement p_currentAdvancement)
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

	protected override float EvaluateEventProgress(EventBase p_event) => 0f;
}