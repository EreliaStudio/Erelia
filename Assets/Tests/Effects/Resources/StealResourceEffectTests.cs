using NUnit.Framework;

namespace Tests.Effects
{

public sealed class StealResourceEffectTests : EffectTestBase
{
	[Test]
	public void Apply_StealsHealth()
	{
		BattleUnit source = CreateUnit(p_health: 100);
		BattleUnit target = CreateUnit(p_health: 100);
		source.BattleAttributes.Health.Decrease(50);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.Health,
			Value = 10
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.Health.Current, Is.EqualTo(60));
		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(90));
	}

	[Test]
	public void Apply_StealHealthDoesNotDamageTargetBelowZero()
	{
		BattleUnit source = CreateUnit(p_health: 100);
		BattleUnit target = CreateUnit(p_health: 20);
		source.BattleAttributes.Health.Decrease(50);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.Health,
			Value = 50
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(0));
	}

	[Test]
	public void Apply_StealHealthDoesNotHealSourceAboveMax()
	{
		BattleUnit source = CreateUnit(p_health: 100);
		BattleUnit target = CreateUnit(p_health: 100);
		source.BattleAttributes.Health.Decrease(5);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.Health,
			Value = 50
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.Health.Current, Is.EqualTo(100));
	}

	[Test]
	public void Apply_StealsActionPoints()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		source.BattleAttributes.ActionPoints.Set(0, 6, true);
		target.BattleAttributes.ActionPoints.Set(4, 6, true);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.ActionPoint,
			Value = 3
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.ActionPoints.Current, Is.EqualTo(3));
		Assert.That(target.BattleAttributes.ActionPoints.Current, Is.EqualTo(1));
	}

	[Test]
	public void Apply_StealsMovementPoints()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		source.BattleAttributes.MovementPoints.Set(0, 6, true);
		target.BattleAttributes.MovementPoints.Set(4, 6, true);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.MovementPoint,
			Value = 3
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.MovementPoints.Current, Is.EqualTo(3));
		Assert.That(target.BattleAttributes.MovementPoints.Current, Is.EqualTo(1));
	}

	[Test]
	public void Apply_StealsRange()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		source.BattleAttributes.BonusRange.Set(0, true);
		target.BattleAttributes.BonusRange.Set(5, true);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.Range,
			Value = 3
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.BonusRange.Value, Is.EqualTo(3));
		Assert.That(target.BattleAttributes.BonusRange.Value, Is.EqualTo(2));
	}

	[Test]
	public void Apply_StealsTurnBarTime()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		source.BattleAttributes.TurnBar.Set(0f, 4f, true);
		target.BattleAttributes.TurnBar.Set(3f, 4f, true);

		new StealResourceEffect
		{
			ResourceTargeted = StealResourceEffect.Target.Stamina,
			Value = 2
		}.Apply(CreateContext(source, target));

		Assert.That(source.BattleAttributes.TurnBar.Current, Is.EqualTo(2f));
		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(1f));
	}
}


}