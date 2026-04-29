using NUnit.Framework;

namespace Tests.Effects
{

public sealed class DamageTargetEffectTests : EffectTestBase
{
	[Test]
	public void Apply_DamagesHealthAndRecordsEvents()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);

		CreateDamageEffect(15)
			.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(85));
		Assert.That(FindEvent<DealDamageRequirement.Event>(source)?.Amount, Is.EqualTo(15));
		Assert.That(FindEvent<TakeDamageRequirement.Event>(target)?.Amount, Is.EqualTo(15));
	}

	[Test]
	public void Apply_DoesNotReduceHealthBelowZero()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 20);

		CreateDamageEffect(50)
			.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(0));
	}

	[Test]
	public void Apply_PhysicalShieldAbsorbsDamageBeforeHealth()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);

		CreateDamageEffect(15)
			.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(95));
	}

	[Test]
	public void Apply_PhysicalShieldCanAbsorbAllDamage()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

		CreateDamageEffect(15)
			.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(100));
	}

	[Test]
	public void Apply_LifeStealOnlyUsesDamageDealtToHealth()
	{
		BattleUnit source = CreateUnit(p_health: 100, p_lifeSteal: 1f);
		BattleUnit target = CreateUnit(p_health: 100);

		source.BattleAttributes.Health.Decrease(50);
		target.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);

		CreateDamageEffect(15)
			.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(95));
		Assert.That(source.BattleAttributes.Health.Current, Is.EqualTo(55));
	}

	[Test]
	public void Apply_LifeStealDoesNotHealAboveMaxHealth()
	{
		BattleUnit source = CreateUnit(p_health: 100, p_lifeSteal: 1f);
		BattleUnit target = CreateUnit(p_health: 100);

		source.BattleAttributes.Health.Decrease(5);

		CreateDamageEffect(20)
			.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.Health.Current, Is.EqualTo(100));
	}
}


}