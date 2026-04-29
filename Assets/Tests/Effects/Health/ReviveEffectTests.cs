using NUnit.Framework;
using Tests.Effects;

namespace Tests.Effects.Health
{
	public sealed class ReviveEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_RestoresHealthWhenTargetIsDefeated()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(p_health: 20);
			target.BattleAttributes.Health.Decrease(20);

			new ReviveEffect { HealthRestored = 7 }
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(7));
		}

		[Test]
		public void Apply_DoesNotRestoreMoreThanMaxHealth()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(p_health: 20);
			target.BattleAttributes.Health.Decrease(20);

			new ReviveEffect { HealthRestored = 50 }
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(20));
		}

		[Test]
		public void Apply_WithZeroHealthRestored_RevivesWithMinimumHealth()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(p_health: 20);
			target.BattleAttributes.Health.Decrease(20);

			new ReviveEffect { HealthRestored = 0 }
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(1));
		}

		[Test]
		public void Apply_WhenTargetIsNotDefeated_DoesNotChangeHealth()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(p_health: 20);
			target.BattleAttributes.Health.Decrease(5);

			new ReviveEffect { HealthRestored = 7 }
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(15));
		}
	}
}
