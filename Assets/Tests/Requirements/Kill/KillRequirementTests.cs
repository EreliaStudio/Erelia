using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Kill.KillCount
{
	public sealed class KillCountTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<Status>() },
			BattleSide.Player);

		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new KillCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneKill_PartialProgress()
		{
			var req = new KillCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit() }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f / 3f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new KillCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit() },
				new UnitDefeatedEvent { Caster = CreateTestUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AbilityFilter_MatchingAbility_CountsProgress()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new KillCountRequirement { SourceAbilities = new List<Ability> { ability }, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit(), SourceAbility = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void AbilityFilter_WrongAbility_ZeroProgress()
		{
			Ability abilityA = ScriptableObject.CreateInstance<Ability>();
			Ability abilityB = ScriptableObject.CreateInstance<Ability>();
			var req = new KillCountRequirement { SourceAbilities = new List<Ability> { abilityA }, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit(), SourceAbility = abilityB }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
			Object.DestroyImmediate(abilityA);
			Object.DestroyImmediate(abilityB);
		}

		[Test]
		public void AbilityFilter_EmptyFilter_AnyAbilityCounts()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new KillCountRequirement { SourceAbilities = new List<Ability>(), RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit(), SourceAbility = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}
	}
}

namespace Tests.Requirements.Kill.LastHit
{
	public sealed class LastHitTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<Status>() },
			BattleSide.Player);

		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new LastHitRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneKill_PartialProgress()
		{
			var req = new LastHitRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit() }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(20f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new LastHitRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new UnitDefeatedEvent { Caster = CreateTestUnit() },
				new UnitDefeatedEvent { Caster = CreateTestUnit() },
				new UnitDefeatedEvent { Caster = CreateTestUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
