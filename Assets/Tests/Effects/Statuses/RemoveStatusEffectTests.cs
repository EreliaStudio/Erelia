using NUnit.Framework;

namespace Tests.Effects
{

public sealed class RemoveStatusEffectTests : EffectTestBase
{
	[Test]
	public void Apply_RemovesRequestedStacks()
	{
		BattleUnit target = CreateUnit();
		Status status = CreateStatus("burn");
		target.Statuses.Add(status, p_stackCount: 3);

		new RemoveStatusEffect
		{
			Status = status,
			StackCount = 2
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Count, Is.EqualTo(1));
		Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(1));
	}

	[Test]
	public void Apply_WhenRemovedStacksEqualCurrentStacks_RemovesStatus()
	{
		BattleUnit target = CreateUnit();
		Status status = CreateStatus("burn");
		target.Statuses.Add(status, p_stackCount: 3);

		new RemoveStatusEffect
		{
			Status = status,
			StackCount = 3
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(status), Is.False);
		Assert.That(target.Statuses.Count, Is.EqualTo(0));
	}

	[Test]
	public void Apply_WhenRemovedStacksExceedCurrentStacks_RemovesStatus()
	{
		BattleUnit target = CreateUnit();
		Status status = CreateStatus("burn");
		target.Statuses.Add(status, p_stackCount: 3);

		new RemoveStatusEffect
		{
			Status = status,
			StackCount = 10
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(status), Is.False);
		Assert.That(target.Statuses.Count, Is.EqualTo(0));
	}

	[Test]
	public void Apply_DoesNotRemoveOtherStatuses()
	{
		BattleUnit target = CreateUnit();
		Status burn = CreateStatus("burn");
		Status guard = CreateStatus("guard");
		target.Statuses.Add(burn, p_stackCount: 2);
		target.Statuses.Add(guard, p_stackCount: 2);

		new RemoveStatusEffect
		{
			Status = burn,
			StackCount = 1
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(burn), Is.True);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_WhenTargetDoesNotHaveStatus_DoesNotChangeStatuses()
	{
		BattleUnit target = CreateUnit();
		Status burn = CreateStatus("burn");
		Status guard = CreateStatus("guard");
		target.Statuses.Add(guard, p_stackCount: 2);

		new RemoveStatusEffect
		{
			Status = burn,
			StackCount = 1
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Count, Is.EqualTo(1));
		Assert.That(target.Statuses.Contains(guard), Is.True);
		Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
	}
}


}