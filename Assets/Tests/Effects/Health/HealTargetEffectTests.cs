using NUnit.Framework;

namespace Tests.Effects
{

public sealed class HealTargetEffectTests : EffectTestBase
{
	[Test]
	public void Apply_HealsTargetAndRecordsCasterEvent()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.Health.Decrease(40);

		new HealTargetEffect
		{
			Input = new MathFormula.HealingInput
			{
				BaseHealing = 15,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(75));
		Assert.That(FindEvent<HealHealthRequirement.Event>(source)?.Amount, Is.EqualTo(15));
	}

	[Test]
	public void Apply_DoesNotHealAboveMaxHealth()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.Health.Decrease(5);

		new HealTargetEffect
		{
			Input = new MathFormula.HealingInput
			{
				BaseHealing = 20,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(100));
		Assert.That(FindEvent<HealHealthRequirement.Event>(source)?.Amount, Is.EqualTo(5));
	}

	[Test]
	public void Apply_WithZeroHealing_DoesNotChangeHealth()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.Health.Decrease(40);

		new HealTargetEffect
		{
			Input = new MathFormula.HealingInput
			{
				BaseHealing = 0,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(60));
	}
}


}