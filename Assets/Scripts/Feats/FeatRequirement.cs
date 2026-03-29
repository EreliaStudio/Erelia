using System;

[Serializable]
public abstract class FeatRequirement
{
	[Serializable]
	public abstract class EventBase
	{
	}

	public abstract float Register(EventBase p_event);
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
		return 0;
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
		return 0;
	}
}
