using NUnit.Framework;
using System.Collections.Generic;

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
	public void Apply_ActionPointsCanExceedMax()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.ActionPoints.Set(4, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.ActionPoint,
			Value = 10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.ActionPoints.Current, Is.EqualTo(14));
	}

	[Test]
	public void Apply_ActionPointLossFiresStatusHook()
	{
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.ActionPoints.Set(4, 6, true);

		Status status = CreateStatus();
		status.HookPoint = StatusHookPoint.OnAPLoss;
		status.Effects = new List<Effect> { CreateDamageEffect(5) };
		target.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.ActionPoint,
			Value = -3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(95));
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
	public void Apply_MovementPointsCanExceedMax()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.MovementPoints.Set(4, 6, true);

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.MovementPoint,
			Value = 10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.MovementPoints.Current, Is.EqualTo(14));
	}

	[Test]
	public void Apply_MovementPointLossFiresStatusHook()
	{
		BattleUnit target = CreateUnit(p_health: 100);
		target.BattleAttributes.MovementPoints.Set(4, 6, true);

		Status status = CreateStatus();
		status.HookPoint = StatusHookPoint.OnMPLoss;
		status.Effects = new List<Effect> { CreateDamageEffect(5) };
		target.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });

		new ResourceChangeEffect
		{
			ResourceTargeted = ResourceChangeEffect.Target.MovementPoint,
			Value = -3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(95));
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
