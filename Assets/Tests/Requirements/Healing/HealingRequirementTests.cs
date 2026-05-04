using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Healing.HealSelf
{
	public sealed class HealSelfTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void AmountBelowRequired_PartialProgress()
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
		public void AmountAtRequired_Completes()
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
		public void AllyEvent_Ignored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 50, IsSelf = false }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Healing.HealOther
{
	public sealed class HealOtherTests
	{
		[Test]
		public void AmountBelowRequired_PartialProgress()
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
		public void AmountAtRequired_Completes()
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
		public void SelfEvent_Ignored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 40, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealTargetRequirement.Event { Amount = 40, IsSelf = true }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Healing.WinAfterHealing
{
	public sealed class WinAfterHealingTests
	{
		[Test]
		public void HealAmountAccumulatesProgress()
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
		public void ReachingThreshold_Completes()
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
