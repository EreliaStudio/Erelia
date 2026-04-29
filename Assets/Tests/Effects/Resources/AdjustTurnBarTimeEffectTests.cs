using NUnit.Framework;

namespace Tests.Effects
{

public sealed class AdjustTurnBarTimeEffectTests : EffectTestBase
{
	[Test]
	public void Apply_IncreasesCurrentTurnBarTime()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.TurnBar.Set(1f, 4f, true);

		new AdjustTurnBarTimeEffect
		{
			Delta = 2f
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(3f));
		Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
	}

	[Test]
	public void Apply_DecreasesCurrentTurnBarTime()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.TurnBar.Set(3f, 4f, true);

		new AdjustTurnBarTimeEffect
		{
			Delta = -2f
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(1f));
		Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
	}

	[Test]
	public void Apply_WithZeroDelta_DoesNotChangeCurrentTurnBarTime()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.TurnBar.Set(2f, 4f, true);

		new AdjustTurnBarTimeEffect
		{
			Delta = 0f
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(2f));
		Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
	}

	[Test]
	public void Apply_WhenDeltaWouldGoBelowZero_ClampsCurrentAtZero()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.TurnBar.Set(1f, 4f, true);

		new AdjustTurnBarTimeEffect
		{
			Delta = -10f
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(0f));
		Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
	}

	[Test]
	public void Apply_WhenDeltaWouldExceedMax_ClampsCurrentAtMax()
	{
		BattleUnit target = CreateUnit();
		target.BattleAttributes.TurnBar.Set(3f, 4f, true);

		new AdjustTurnBarTimeEffect
		{
			Delta = 10f
		}.Apply(CreateContext(p_target: target));

		Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(4f));
		Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
	}

	[Test]
	public void Apply_WhenTargetIsNull_DoesNotThrow()
	{
		Assert.DoesNotThrow(() =>
		{
			new AdjustTurnBarTimeEffect
			{
				Delta = 2f
			}.Apply(CreateContext());
		});
	}
}

}