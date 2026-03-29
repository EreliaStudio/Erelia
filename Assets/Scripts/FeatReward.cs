using System;

[Serializable]
public abstract class FeatReward
{
	public abstract void Apply(CreatureUnit creatureUnit);
}

[Serializable]
public class BonusStatsReward : FeatReward
{
	public enum AttributeType
	{
		Health,
		ActionPoints,
		Movement,
		Attack,
		Armor,
		Magic,
		Resistance,
		BonusRange,
		Recovery
	};

	public AttributeType Attribute;
	public int Value = 1;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		
	}
}

[Serializable]
public class AbilityReward : FeatReward
{
	public Ability Ability;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		
	}
}

[Serializable]
public class PassiveReward : FeatReward
{
	public Status Status;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		
	}
}

[Serializable]
public class ChangeFormReward : FeatReward
{
	public string FormKey;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		
	}
}