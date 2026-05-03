using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.AbilityCasting
{
	public sealed class CastAbilityCountRequirementTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new CastAbilityCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void OneEvent_PartialProgress()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new CastAbilityCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new CastAbilityCountRequirement.Event { Ability = ability }
			});

			// 1/3 ≈ 33.33%
			Assert.That(progress.CurrentProgress, Is.EqualTo(100f / 3f).Within(0.01f));
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void ThreeEvents_Completes()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new CastAbilityCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new CastAbilityCountRequirement.Event { Ability = ability },
				new CastAbilityCountRequirement.Event { Ability = ability },
				new CastAbilityCountRequirement.Event { Ability = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void AbilityFilter_MatchingAbility_Returns100()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new CastAbilityCountRequirement { Abilities = new List<Ability> { ability }, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new CastAbilityCountRequirement.Event { Ability = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}

		[Test]
		public void AbilityFilter_WrongAbility_ZeroProgress()
		{
			Ability abilityA = ScriptableObject.CreateInstance<Ability>();
			Ability abilityB = ScriptableObject.CreateInstance<Ability>();
			var req = new CastAbilityCountRequirement { Abilities = new List<Ability> { abilityA }, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new CastAbilityCountRequirement.Event { Ability = abilityB }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
			Object.DestroyImmediate(abilityA);
			Object.DestroyImmediate(abilityB);
		}

		[Test]
		public void AbilityFilter_EmptyList_AnyAbilityCompletes()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new CastAbilityCountRequirement { Abilities = new List<Ability>(), RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new CastAbilityCountRequirement.Event { Ability = ability }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(ability);
		}
	}
}
