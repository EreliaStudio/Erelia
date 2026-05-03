using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Healing
{
	public sealed class HealingRequirementTests
	{
		// ── HealTargetRequirement (Self) ──────────────────────────────────────────

		[Test]
		public void HealSelf_AmountBelowRequired_PartialProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 100, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 30, IsSelf = true }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(30f).Within(0.01f));
		}

		[Test]
		public void HealSelf_AmountAtRequired_Completes()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 50, IsSelf = true }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void HealSelf_NoEvents_ZeroProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void HealSelf_AllyEventIgnored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 50, IsSelf = false }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		// ── HealTargetRequirement (Ally) ──────────────────────────────────────────

		[Test]
		public void HealOther_AmountBelowRequired_PartialProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 100, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 75, IsSelf = false }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(75f).Within(0.01f));
		}

		[Test]
		public void HealOther_AmountAtRequired_Completes()
		{
			var req = new HealTargetRequirement { RequiredAmount = 40, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 40, IsSelf = false }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void HealOther_SelfEventIgnored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 40, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 40, IsSelf = true }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		// ── WinAfterHealingRequirement ────────────────────────────────────────────

		[Test]
		public void WinAfterHealing_HealAmountAccumulatesProgress()
		{
			var req = new WinAfterHealingRequirement { RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealHealthRequirement.Event { Amount = 50 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void WinAfterHealing_ReachingThresholdCompletes()
		{
			var req = new WinAfterHealingRequirement { RequiredAmount = 30 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealHealthRequirement.Event { Amount = 30 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
