using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.DamageEffect
{
	public sealed class DamageEffectTests
	{
		[Test]
		public void Apply_RecordsDealDamageEventOnSourceUnit()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

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

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.EqualTo(1));
			Assert.That(sourceUnit.PendingFeatEvents[0], Is.InstanceOf<DealDamageRequirement.Event>());
			var dealEvent = (DealDamageRequirement.Event)sourceUnit.PendingFeatEvents[0];
			Assert.That(dealEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void Apply_RecordsTakeDamageEventOnTargetUnit()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

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

			Assert.That(targetUnit.PendingFeatEvents.Count, Is.EqualTo(2));
			Assert.That(targetUnit.PendingFeatEvents[0], Is.InstanceOf<TakeDamageRequirement.Event>());
			var takeEvent = (TakeDamageRequirement.Event)targetUnit.PendingFeatEvents[0];
			Assert.That(takeEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void Apply_BothEventsCarryTheSameAppliedAmount()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 5);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

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

			int dealtAmount = ((DealDamageRequirement.Event)sourceUnit.PendingFeatEvents[0]).Amount;
			int takenAmount = ((TakeDamageRequirement.Event)targetUnit.PendingFeatEvents[0]).Amount;

			Assert.That(dealtAmount, Is.EqualTo(5));
			Assert.That(takenAmount, Is.EqualTo(5));
		}

		[Test]
		public void Apply_RecordsNoEventsWhenBaseDamageIsZero()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

			var effect = new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput { BaseDamage = 0 }
			};

			effect.Apply(context);

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
			Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
		}

		[Test]
		public void Apply_RecordsNoEventsWhenTargetAlreadyAtZeroHealth()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 0);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

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

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
			Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
		}

		private static BattleUnit CreateUnit(BattleSide side, int health = 100)
		{
			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = health },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
			return new BattleUnit(creatureUnit, side);
		}

		private static BattleAbilityExecutionContext CreateContext(BattleUnit source, BattleUnit target)
		{
			return new BattleAbilityExecutionContext { SourceObject = source, TargetObject = target };
		}
	}
}
