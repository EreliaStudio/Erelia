using NUnit.Framework;

namespace Tests.Effects
{

public sealed class ResourceChangeEffectTests : EffectTestBase
{
	[Test]
	public void Apply_ChangesActionPoints()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.ActionPoints.Set(4, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.ActionPoint,
			Value = -3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.ActionPoints.Current, Is.EqualTo(1));
	}

	[Test]
	public void Apply_ActionPointsDoNotGoBelowZero()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.ActionPoints.Set(2, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.ActionPoint,
			Value = -10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.ActionPoints.Current, Is.EqualTo(0));
	}

	[Test]
	public void Apply_ActionPointsDoNotExceedMax()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.ActionPoints.Set(4, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.ActionPoint,
			Value = 10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.ActionPoints.Current, Is.EqualTo(6));
	}

	[Test]
	public void Apply_ChangesMovementPoints()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.MovementPoints.Set(1, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.MovementPoint,
			Value = 3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.MovementPoints.Current, Is.EqualTo(4));
	}

	[Test]
	public void Apply_MovementPointsDoNotGoBelowZero()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.MovementPoints.Set(2, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.MovementPoint,
			Value = -10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.MovementPoints.Current, Is.EqualTo(0));
	}

	[Test]
	public void Apply_MovementPointsDoNotExceedMax()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.MovementPoints.Set(4, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.MovementPoint,
			Value = 10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.MovementPoints.Current, Is.EqualTo(6));
	}

	[Test]
	public void Apply_ChangesBonusRangeAndClampsAtZero()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.BonusRange.Set(2, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.Range,
			Value = -5
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.BonusRange.Value, Is.EqualTo(0));
	}

	[Test]
	public void Apply_IncreasesBonusRange()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.BonusRange.Set(2, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.Range,
			Value = 3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.BonusRange.Value, Is.EqualTo(5));
	}
}


}