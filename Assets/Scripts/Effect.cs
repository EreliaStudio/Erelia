using System;
using UnityEngine;

[Serializable]
public abstract class Effect
{
	public abstract void Apply(CreatureUnit caster, CreatureUnit target);
}

[Serializable]
public class DamageTargetEffect : Effect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		target.Attributes.Health.Value -= Value;
	}
}

[Serializable]
public class HealTargetEffect : Effect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		target.Attributes.Health.Value += Value;
	}
}