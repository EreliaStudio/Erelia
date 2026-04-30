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
	}

	public Scope RequirementScope = Scope.Fight;
	public int RequiredRepeatCount = 1;

	public Advancement EvaluateEvents(
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
		float fightProgress = 0f;

		for (int index = 0; index < p_events.Count; index++)
		{
			fightProgress += GetEventProgress(p_events[index]);
		}

		if (fightProgress >= 100f)
		{
			RegisterOneCompletion(ref p_advancement);
		}
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
		public int Count = 1;
	}

	public Ability Ability;
	public int RequiredCount = 1;

	protected override float EvaluateProgress(Event p_event)
	{
		if (Ability != null && p_event.Ability != Ability)
		{
			return 0f;
		}

		return ComputeLinearProgress(p_event.Count, RequiredCount);
	}
}

[Serializable]
public class CastMultipleAbilitiesInOneTurnRequirement : FeatRequirementTemplated<CastMultipleAbilitiesInOneTurnRequirement.Event>
{
	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public Ability Ability;
		public int AbilityCastCountThisTurn = 0;
		public int TotalCastCountThisTurn = 0;
	}

	public Ability Ability;
	public int RequiredCount = 2;

	public CastMultipleAbilitiesInOneTurnRequirement()
	{
		RequirementScope = Scope.Ability;
	}

	protected override float EvaluateProgress(Event p_event)
	{
		int count = Ability != null
			? p_event.Ability == Ability ? p_event.AbilityCastCountThisTurn : 0
			: p_event.TotalCastCountThisTurn;

		return ComputeLinearProgress(count, RequiredCount);
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
public class MaxSingleHitDamageRequirement : FeatRequirementTemplated<MaxSingleHitDamageRequirement.Event>
{
	public int RequiredAmount = 10;

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int Amount = 0;
	}

	public MaxSingleHitDamageRequirement()
	{
		RequirementScope = Scope.Ability;
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
