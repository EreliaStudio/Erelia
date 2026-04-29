using System;
using UnityEngine;

[Serializable]
public abstract class FeatRequirement
{
	public enum ProgressMode { Additive, Maximum }

	[Serializable]
	public abstract class EventBase
	{
	}

	public virtual ProgressMode Mode => ProgressMode.Additive;

	public abstract float Register(EventBase p_event);

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

		return Mathf.Clamp((float)p_amount / p_requiredAmount * 100f, 0f, 100f);
	}
}

[Serializable]
public abstract class FeatRequirementTemplated<TEvent> : FeatRequirement
	where TEvent : FeatRequirement.EventBase
{
	public sealed override float Register(EventBase p_event)
	{
		if (p_event is TEvent typedEvent == false)
		{
			return 0f;
		}

		return ComputeProgress(typedEvent);
	}

	protected abstract float ComputeProgress(TEvent p_event);
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

	protected override float ComputeProgress(Event p_event)
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

	protected override float ComputeProgress(Event p_event)
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

	protected override float ComputeProgress(Event p_event)
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

	public override ProgressMode Mode => ProgressMode.Maximum;

	protected override float ComputeProgress(Event p_event)
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

	protected override float ComputeProgress(Event p_event)
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

	public override ProgressMode Mode => ProgressMode.Maximum;

	protected override float ComputeProgress(Event p_event)
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

	public override ProgressMode Mode => ProgressMode.Maximum;

	protected override float ComputeProgress(Event p_event)
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

	public override ProgressMode Mode => ProgressMode.Maximum;

	protected override float ComputeProgress(Event p_event)
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

	protected override float ComputeProgress(Event p_event)
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

	protected override float ComputeProgress(Event p_event)
	{
		return ComputeLinearProgress(p_event.Amount, RequiredAmount);
	}
}

[Serializable]
public class MaxDamageAbsorbedInOneHitRequirement : FeatRequirementTemplated<AbsorbDamageWithShieldRequirement.Event>
{
	public int RequiredAmount = 10;

	public override ProgressMode Mode => ProgressMode.Maximum;

	protected override float ComputeProgress(AbsorbDamageWithShieldRequirement.Event p_event)
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

	protected override float ComputeProgress(Event p_event)
	{
		if (Filter == KindFilter.Physical && p_event.Kind != ShieldKind.Physical) return 0f;
		if (Filter == KindFilter.Magical && p_event.Kind != ShieldKind.Magical) return 0f;
		return ComputeLinearProgress(1, RequiredCount);
	}
}
