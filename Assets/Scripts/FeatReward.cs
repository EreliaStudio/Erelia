using System;

[Serializable]
public abstract class FeatReward
{
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
}

[Serializable]
public class AbilityReward : FeatReward
{
	public Ability Ability;
}

[Serializable]
public class PassiveReward : FeatReward
{
	public string PassiveName;
}

[Serializable]
public class ChangeFormReward : FeatReward
{
	public string FormKey;
}