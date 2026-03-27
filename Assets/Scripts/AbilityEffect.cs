using System;
using UnityEngine;

[Serializable]
public abstract class AbilityEffect
{
	public abstract void Apply(CreatureUnit caster, CreatureUnit target);
}

[Serializable]
public class DamageTargetEffect : AbilityEffect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		target.Attributes.Health.Value -= Value;
	}
}

[Serializable]
public class HealTargetEffect : AbilityEffect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		target.Attributes.Health.Value += Value;
	}
}