using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.NonDamageEffect
{
	public sealed class NonDamageEffectTests
	{
		[Test]
		public void Apply_RecordsNoEvents()
		{
			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
			BattleUnit sourceUnit = new BattleUnit(creatureUnit, BattleSide.Player);
			BattleUnit targetUnit = new BattleUnit(new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			}, BattleSide.Enemy);

			BattleAbilityExecutionContext context = new BattleAbilityExecutionContext
			{
				SourceObject = sourceUnit,
				TargetObject = targetUnit
			};

			var effect = new AdjustTurnBarTimeEffect { Delta = 1f };
			effect.Apply(context);

			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
			Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
		}
	}
}
