using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Damage
{
	public sealed class DamageRequirementTests
	{
		// ── DealDamageRequirement – ability filter ────────────────────────────────

		[Test]
		public void DealDamageWithAbility_MatchingAbility_CountsProgress()
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
		public void DealDamageWithAbility_WrongAbility_ZeroProgress()
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
		public void DealDamageWithAbility_EmptyAbilitiesFilter_AnyAbilityCounts()
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

		// ── DealDamageRequirement – damage kind filter ────────────────────────────

		[Test]
		public void DealDamageWithKind_MatchingKind_CountsProgress()
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
		public void DealDamageWithKind_WrongKind_ZeroProgress()
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

		// ── WinAfterDealingDamageRequirement ─────────────────────────────────────

		[Test]
		public void WinAfterDealing_DamageAccumulatesProgress()
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
		public void WinAfterDealing_ReachingThresholdCompletes()
		{
			var req = new WinAfterDealingDamageRequirement { RequiredAmount = 50 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		// ── SurviveHitRequirement ─────────────────────────────────────────────────

		[Test]
		public void SurviveHit_AmountAboveThreshold_Completes()
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
		public void SurviveHit_AmountBelowThreshold_ZeroProgress()
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
		public void SurviveHit_AmountExactlyAtThreshold_Completes()
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
