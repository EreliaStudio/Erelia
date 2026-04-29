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
	public enum DistanceKind { Within, AtLeast }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int ClosestAllyDistance = int.MaxValue;
		public int ClosestEnemyDistance = int.MaxValue;
	}

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;

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
			_ => false
		};
	}
}

[Serializable]
public class TurnEndPositionRequirement : FeatRequirementTemplated<TurnEndPositionRequirement.Event>
{
	public enum TargetKind { Ally, Enemy, AnyUnit }
	public enum DistanceKind { Within, AtLeast }

	[Serializable]
	public class Event : FeatRequirement.EventBase
	{
		public int ClosestAllyDistance = int.MaxValue;
		public int ClosestEnemyDistance = int.MaxValue;
	}

	public TargetKind Target = TargetKind.Enemy;
	public DistanceKind Condition = DistanceKind.Within;
	public int Distance = 3;

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
			_ => false
		};
	}
}
