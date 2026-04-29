using NUnit.Framework;
using Tests.Effects;

namespace Tests.Effects.Resources
{
	public sealed class AdjustTurnBarDurationEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_IncreasesTurnBarDuration()
		{
			BattleUnit target = CreateUnit();
			target.BattleAttributes.TurnBar.Set(1f, 4f, true);

			new AdjustTurnBarDurationEffect { Delta = 2f }
				.Apply(CreateContext(p_target: target));

			Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(6f));
			Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(1f));
		}

		[Test]
		public void Apply_DecreasesTurnBarDuration()
		{
			BattleUnit target = CreateUnit();
			target.BattleAttributes.TurnBar.Set(1f, 6f, true);

			new AdjustTurnBarDurationEffect { Delta = -2f }
				.Apply(CreateContext(p_target: target));

			Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(4f));
			Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(1f));
		}

		[Test]
		public void Apply_WithZeroDelta_DoesNotChangeTurnBarDuration()
		{
			BattleUnit target = CreateUnit();
			target.BattleAttributes.TurnBar.Set(2f, 5f, true);

			new AdjustTurnBarDurationEffect { Delta = 0f }
				.Apply(CreateContext(p_target: target));

			Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(5f));
			Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(2f));
		}

		[Test]
		public void Apply_WhenDurationIsReducedBelowCurrent_ClampsCurrentToNewMax()
		{
			BattleUnit target = CreateUnit();
			target.BattleAttributes.TurnBar.Set(5f, 6f, true);

			new AdjustTurnBarDurationEffect { Delta = -3f }
				.Apply(CreateContext(p_target: target));

			Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(3f));
			Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(3f));
		}

		[Test]
		public void Apply_WhenDurationWouldGoBelowMinimum_ClampsDurationAtMinimum()
		{
			BattleUnit target = CreateUnit();
			target.BattleAttributes.TurnBar.Set(1f, 4f, true);

			new AdjustTurnBarDurationEffect { Delta = -10f }
				.Apply(CreateContext(p_target: target));

			Assert.That(target.BattleAttributes.TurnBar.Max, Is.EqualTo(0.1f).Within(0.0001f));
			Assert.That(target.BattleAttributes.TurnBar.Current, Is.EqualTo(0.1f).Within(0.0001f));
		}

		[Test]
		public void Apply_WhenTargetIsNull_DoesNotThrow()
		{
			Assert.DoesNotThrow(() =>
			{
				new AdjustTurnBarDurationEffect { Delta = 2f }
					.Apply(CreateContext());
			});
		}
	}
}
