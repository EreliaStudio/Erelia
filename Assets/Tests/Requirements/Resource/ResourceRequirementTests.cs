using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Resource.SpendActionPoints
{
	public sealed class SpendActionPointsTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new SpendActionPointsRequirement { RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void PartialAmount_PartialProgress()
		{
			var req = new SpendActionPointsRequirement { RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SpendActionPointsRequirement.Event { Amount = 4 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(40f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new SpendActionPointsRequirement { RequiredAmount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SpendActionPointsRequirement.Event { Amount = 5 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void MultipleEvents_Accumulate()
		{
			var req = new SpendActionPointsRequirement { RequiredAmount = 6 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SpendActionPointsRequirement.Event { Amount = 2 },
				new SpendActionPointsRequirement.Event { Amount = 4 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}

namespace Tests.Requirements.Resource.SpendMovementPoints
{
	public sealed class SpendMovementPointsTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new SpendMovementPointsRequirement { RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void PartialAmount_PartialProgress()
		{
			var req = new SpendMovementPointsRequirement { RequiredAmount = 8 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SpendMovementPointsRequirement.Event { Amount = 4 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new SpendMovementPointsRequirement { RequiredAmount = 6 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SpendMovementPointsRequirement.Event { Amount = 3 },
				new SpendMovementPointsRequirement.Event { Amount = 3 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
