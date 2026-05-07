using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.DamageEffect
{
	public sealed class DamageEffectTests
	{
		[Test]
		public void Apply_RecordsDamageEventOnSourceUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = 10,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};

			effect.Apply(context);

			Assert.That(capture.Count(sourceUnit), Is.EqualTo(1));
			var damageEvent = capture.Find<DamageEvent>(sourceUnit);
			Assert.That(damageEvent, Is.Not.Null);
			Assert.That(damageEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void Apply_RecordsDamageEventOnTargetUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = 10,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};

			effect.Apply(context);

			Assert.That(capture.Count(targetUnit), Is.EqualTo(2));
			var damageEvent = capture.Find<DamageEvent>(targetUnit);
			Assert.That(damageEvent, Is.Not.Null);
			Assert.That(damageEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void Apply_BothLogsCarryTheSameAppliedAmount()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			targetUnit.BattleAttributes.Health.SetCurrent(5);
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = 20,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};

			effect.Apply(context);

			int casterAmount = capture.Find<DamageEvent>(sourceUnit).Amount;
			int targetAmount = capture.Find<DamageEvent>(targetUnit).Amount;

			Assert.That(casterAmount, Is.EqualTo(5));
			Assert.That(targetAmount, Is.EqualTo(5));
		}

		[Test]
		public void Apply_RecordsNoEventsWhenBaseDamageIsZero()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput { BaseDamage = 0 }
			};

			effect.Apply(context);

			Assert.That(capture.Count(sourceUnit), Is.Zero);
			Assert.That(capture.Count(targetUnit), Is.Zero);
		}

		[Test]
		public void Apply_RecordsNoEventsWhenTargetAlreadyAtZeroHealth()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];
			targetUnit.BattleAttributes.Health.SetCurrent(0);
			BattleAbilityExecutionContext context = CreateContext(fixture.BattleContext, sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = 10,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};

			effect.Apply(context);

			Assert.That(capture.Count(sourceUnit), Is.Zero);
			Assert.That(capture.Count(targetUnit), Is.Zero);
		}

		private static BattleAbilityExecutionContext CreateContext(BattleContext battleContext, BattleUnit source, BattleUnit target)
		{
			return new BattleAbilityExecutionContext { BattleContext = battleContext, SourceObject = source, TargetObject = target };
		}
	}
}
