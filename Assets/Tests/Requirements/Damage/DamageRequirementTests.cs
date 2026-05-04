using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Damage.DealDamage
{
	public sealed class DealDamageTests
	{
		[Test]
		public void EmptyAbilitiesFilter_AnyAbilityCounts()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new DealDamageRequirement { SourceAbilities = new List<Ability>(), RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 100, SourceAbility = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void MatchingAbility_CountsProgress()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new DealDamageRequirement { SourceAbilities = new List<Ability> { ability }, RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50, SourceAbility = ability }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void WrongAbility_ZeroProgress()
		{
			Ability abilityA = ScriptableObject.CreateInstance<Ability>();
			Ability abilityB = ScriptableObject.CreateInstance<Ability>();
			var req = new DealDamageRequirement { SourceAbilities = new List<Ability> { abilityA }, RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50, SourceAbility = abilityB }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
			Object.DestroyImmediate(abilityA);
			Object.DestroyImmediate(abilityB);
		}

		[Test]
		public void MatchingKind_CountsProgress()
		{
			var req = new DealDamageRequirement
			{
				FilterByDamageKind = true,
				RequiredDamageKind = MathFormula.DamageInput.Kind.Physical,
				RequiredAmount = 100
			};
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 40, DamageKind = MathFormula.DamageInput.Kind.Physical }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(40f).Within(0.01f));
		}

		[Test]
		public void WrongKind_ZeroProgress()
		{
			var req = new DealDamageRequirement
			{
				FilterByDamageKind = true,
				RequiredDamageKind = MathFormula.DamageInput.Kind.Physical,
				RequiredAmount = 100
			};
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 40, DamageKind = MathFormula.DamageInput.Kind.Magical }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}
	}
}

namespace Tests.Requirements.Damage.SurviveHit
{
	public sealed class SurviveHitTests
	{
		[Test]
		public void AmountAboveThreshold_Completes()
		{
			var req = new SurviveHitRequirement { RequiredAmount = 20 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveHitRequirement.Event { Amount = 25 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AmountBelowThreshold_ZeroProgress()
		{
			var req = new SurviveHitRequirement { RequiredAmount = 20 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveHitRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void AmountExactlyAtThreshold_Completes()
		{
			var req = new SurviveHitRequirement { RequiredAmount = 20 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveHitRequirement.Event { Amount = 20 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}

namespace Tests.Requirements.Damage.WinAfterDealing
{
	public sealed class WinAfterDealingTests
	{
		[Test]
		public void DamageAccumulatesProgress()
		{
			var req = new WinAfterDealingDamageRequirement { RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 60 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(60f).Within(0.01f));
		}

		[Test]
		public void ReachingThreshold_Completes()
		{
			var req = new WinAfterDealingDamageRequirement { RequiredAmount = 50 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
