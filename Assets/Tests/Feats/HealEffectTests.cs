using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.HealEffect
{
	public sealed class HealEffectTests
	{
		[Test]
		public void Apply_RecordsHealEventOnSourceUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			targetUnit.BattleAttributes.Health.SetCurrent(50);
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new HealTargetEffect
			{
				Input = new MathFormula.HealingInput
				{
					BaseHealing = 15,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};

			effect.Apply(context);

			Assert.That(capture.Count(sourceUnit), Is.EqualTo(1));
			var healEvent = capture.Find<HealEvent>(sourceUnit);
			Assert.That(healEvent, Is.Not.Null);
			Assert.That(healEvent.Amount, Is.EqualTo(15));
		}

		[Test]
		public void Apply_RecordsHealEventOnTargetUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			targetUnit.BattleAttributes.Health.SetCurrent(50);
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new HealTargetEffect
			{
				Input = new MathFormula.HealingInput { BaseHealing = 15 }
			};

			effect.Apply(context);

			var healEvent = capture.Find<HealEvent>(targetUnit);
			Assert.That(healEvent, Is.Not.Null);
			Assert.That(healEvent.Amount, Is.EqualTo(15));
		}

		[Test]
		public void Apply_RecordsNoEventsWhenBaseHealingIsZero()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			targetUnit.BattleAttributes.Health.SetCurrent(50);
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new HealTargetEffect
			{
				Input = new MathFormula.HealingInput { BaseHealing = 0 }
			};

			effect.Apply(context);

			Assert.That(capture.Count(sourceUnit), Is.Zero);
		}

		private static BattleAbilityExecutionContext CreateContext(BattleContext battleContext, BattleUnit source, BattleUnit target)
		{
			return new BattleAbilityExecutionContext { BattleContext = battleContext, SourceObject = source, TargetObject = target };
		}
	}
}
