using System;

[Serializable]
public abstract class FeatRequirement
{
}

[Serializable]
public class DealDamageRequirement : FeatRequirement
{
	public int RequiredAmount = 10;
}

[Serializable]
public class HealHealthRequirement : FeatRequirement
{
	public int RequiredAmount = 10;
}