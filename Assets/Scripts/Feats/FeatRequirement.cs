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
