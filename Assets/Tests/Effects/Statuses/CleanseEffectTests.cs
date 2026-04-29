using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Effects
{

public sealed class CleanseEffectTests : EffectTestBase
{
	[Test]
	public void Apply_RemovesStatusesByTag()
	{
		BattleUnit target = CreateUnit();
		Status poison = CreateStatus("poison");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(poison);
		target.Statuses.Add(guard);

		new CleanseEffect
		{
			TagsToCleanse = new List<string> { "poison" }
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(poison), Is.False);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_RemovesAllStatusesMatchingAnyProvidedTag()
	{
		BattleUnit target = CreateUnit();
		Status poison = CreateStatus("poison");
		Status burn = CreateStatus("burn");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(poison);
		target.Statuses.Add(burn);
		target.Statuses.Add(guard);

		new CleanseEffect
		{
			TagsToCleanse = new List<string> { "poison", "burn" }
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(poison), Is.False);
		Assert.That(target.Statuses.Contains(burn), Is.False);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_RemovesStatusWhenItHasAtLeastOneMatchingTag()
	{
		BattleUnit target = CreateUnit();
		Status mixed = CreateStatus("poison", "debuff", "magical");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(mixed);
		target.Statuses.Add(guard);

		new CleanseEffect
		{
			TagsToCleanse = new List<string> { "debuff" }
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Contains(mixed), Is.False);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_WhenNoStatusMatches_DoesNotRemoveAnything()
	{
		BattleUnit target = CreateUnit();
		Status poison = CreateStatus("poison");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(poison);
		target.Statuses.Add(guard);

		new CleanseEffect
		{
			TagsToCleanse = new List<string> { "stun" }
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Count, Is.EqualTo(2));
		Assert.That(target.Statuses.Contains(poison), Is.True);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_WithEmptyTagList_DoesNotRemoveAnything()
	{
		BattleUnit target = CreateUnit();
		Status poison = CreateStatus("poison");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(poison);
		target.Statuses.Add(guard);

		new CleanseEffect
		{
			TagsToCleanse = new List<string>()
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.Statuses.Count, Is.EqualTo(2));
		Assert.That(target.Statuses.Contains(poison), Is.True);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_WithNullTagList_DoesNotThrowAndDoesNotRemoveAnything()
	{
		BattleUnit target = CreateUnit();
		Status poison = CreateStatus("poison");
		Status guard = CreateStatus("guard");

		target.Statuses.Add(poison);
		target.Statuses.Add(guard);

		Assert.DoesNotThrow(() =>
		{
			new CleanseEffect
			{
				TagsToCleanse = null
			}.Apply(CreateContext(p_target: target));
		});

		Assert.That(target.Statuses.Count, Is.EqualTo(2));
		Assert.That(target.Statuses.Contains(poison), Is.True);
		Assert.That(target.Statuses.Contains(guard), Is.True);
	}

	[Test]
	public void Apply_WhenTargetHasNoStatuses_DoesNotThrow()
	{
		BattleUnit target = CreateUnit();

		Assert.DoesNotThrow(() =>
		{
			new CleanseEffect
			{
				TagsToCleanse = new List<string> { "poison" }
			}.Apply(CreateContext(p_target: target));
		});

		Assert.That(target.Statuses.Count, Is.EqualTo(0));
	}

	[Test]
	public void Apply_WhenTargetIsNull_DoesNotThrow()
	{
		Assert.DoesNotThrow(() =>
		{
			new CleanseEffect
			{
				TagsToCleanse = new List<string> { "poison" }
			}.Apply(CreateContext());
		});
	}
}

}