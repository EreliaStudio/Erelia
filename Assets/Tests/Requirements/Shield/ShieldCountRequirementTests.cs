using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Shield
{
	public sealed class ShieldCountRequirementTests
	{
		[Test]
		public void ApplyShieldCount_NoEvents_ZeroProgress()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void ApplyShieldCount_OneEvent_PartialProgress()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyShieldCountRequirement.Event()
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f / 3f).Within(0.01f));
		}

		[Test]
		public void ApplyShieldCount_ExactCountRequired_Completes()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyShieldCountRequirement.Event(),
				new ApplyShieldCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void ApplyShieldCount_MoreThanRequired_StillCompletes()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyShieldCountRequirement.Event(),
				new ApplyShieldCountRequirement.Event(),
				new ApplyShieldCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
