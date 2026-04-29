using NUnit.Framework;

namespace Tests.Effects
{

public sealed class ApplyShieldEffectTests : EffectTestBase
{
	[Test]
	public void Apply_AddsShieldToTarget()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = 12,
			DurationInTurns = 2
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Magical));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(12));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.RemainingTurns, Is.EqualTo(2));
	}

	[Test]
	public void Apply_RecordsCasterShieldEvent()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = 12,
			DurationInTurns = 2
		}.Apply(CreateContext(source, target));

		ApplyShieldRequirement.Event shieldEvent = FindEvent<ApplyShieldRequirement.Event>(source);

		Assert.That(shieldEvent, Is.Not.Null);
		Assert.That(shieldEvent.Amount, Is.EqualTo(12));
	}

	[Test]
	public void Apply_CanAddPhysicalShield()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Physical,
			Amount = 8,
			DurationInTurns = 3
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Physical));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(8));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.RemainingTurns, Is.EqualTo(3));
	}

	[Test]
	public void Apply_CanAddMultipleShields()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Physical,
			Amount = 8,
			DurationInTurns = 2
		}.Apply(CreateContext(source, target));

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = 12,
			DurationInTurns = 3
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(2));

		Assert.That(target.BattleAttributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Physical));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(8));

		Assert.That(target.BattleAttributes.ActiveShields[1].Value.Kind, Is.EqualTo(ShieldKind.Magical));
		Assert.That(target.BattleAttributes.ActiveShields[1].Value.CurrentAmount, Is.EqualTo(12));
	}

	[Test]
	public void Apply_WithZeroAmount_DoesNotAddShield()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = 0,
			DurationInTurns = 2
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		Assert.That(FindEvent<ApplyShieldRequirement.Event>(source), Is.Null);
	}

	[Test]
	public void Apply_WithNegativeAmount_DoesNotAddShield()
	{
		BattleUnit source = CreateUnit();
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = -5,
			DurationInTurns = 2
		}.Apply(CreateContext(source, target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		Assert.That(FindEvent<ApplyShieldRequirement.Event>(source), Is.Null);
	}

	[Test]
	public void Apply_WhenTargetIsNull_DoesNotThrowAndDoesNotRecordEvent()
	{
		BattleUnit source = CreateUnit();

		Assert.DoesNotThrow(() =>
		{
			new ApplyShieldEffect
			{
				Kind = ShieldKind.Magical,
				Amount = 12,
				DurationInTurns = 2
			}.Apply(CreateContext(p_source: source));
		});

		Assert.That(FindEvent<ApplyShieldRequirement.Event>(source), Is.Null);
	}

	[Test]
	public void Apply_WhenSourceIsNull_AddsShieldWithoutRecordingCasterEvent()
	{
		BattleUnit target = CreateUnit();

		new ApplyShieldEffect
		{
			Kind = ShieldKind.Magical,
			Amount = 12,
			DurationInTurns = 2
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
		Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(12));
	}
}

}