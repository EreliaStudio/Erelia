using System;

[Serializable]
public abstract class AICondition
{
	public abstract bool IsMet(BattleUnit caster, BattleContext battleContext);
};

[Serializable]
public class EnemyIsAtDistance : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

[Serializable]
public class AllyIsAtDistance : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

[Serializable]
public class HPThreshold : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

[Serializable]
public class HasStatus : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

[Serializable]
public class CanUseAbility : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

[Serializable]
public class ActiveModeIs : AICondition
{
	public override bool IsMet(BattleUnit caster, BattleContext battleContext)
	{
		return false;
	}
}

//Here will live all the different conditions of the AI decision making system such as :
// - If enemy is near X of distance
// - If enemy is away from X distance
// - If status X is placed on Ally
// Etc etc
