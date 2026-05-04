using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Kill.KillCount
{
	public sealed class KillCountTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new KillCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneKill_PartialProgress()
		{
			var req = new KillCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event()
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f / 3f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new KillCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event(),
				new KillCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AbilityFilter_MatchingAbility_CountsProgress()
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			var req = new KillCountRequirement { SourceAbilities = new List<Ability> { ability }, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event { Ability = ability }
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

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event { Ability = abilityB }
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

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event { Ability = ability }
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
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new LastHitRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneKill_PartialProgress()
		{
			var req = new LastHitRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event()
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(20f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new LastHitRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new KillCountRequirement.Event(),
				new KillCountRequirement.Event(),
				new KillCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
