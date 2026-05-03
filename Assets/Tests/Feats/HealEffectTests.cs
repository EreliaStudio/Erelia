using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.HealEffect
{
	public sealed class HealEffectTests
	{
		[Test]
		public void Apply_RecordsHealHealthEventOnSourceUnit()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

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

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.EqualTo(2));
			Assert.That(sourceUnit.PendingFeatEvents[0], Is.InstanceOf<HealHealthRequirement.Event>());
			var healEvent = (HealHealthRequirement.Event)sourceUnit.PendingFeatEvents[0];
			Assert.That(healEvent.Amount, Is.EqualTo(15));
		}

		[Test]
		public void Apply_RecordsNoEventOnTargetUnit()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

			var effect = new HealTargetEffect
			{
				Input = new MathFormula.HealingInput { BaseHealing = 15 }
			};

			effect.Apply(context);

			Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
		}

		[Test]
		public void Apply_RecordsNoEventsWhenBaseHealingIsZero()
		{
			BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
			BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
			BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

			var effect = new HealTargetEffect
			{
				Input = new MathFormula.HealingInput { BaseHealing = 0 }
			};

			effect.Apply(context);

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
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
