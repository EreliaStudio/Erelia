using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Movement.MoveCount
{
	public sealed class MoveCountTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new MoveCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneMove_PartialProgress()
		{
			var req = new MoveCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MoveCountRequirement.Event()
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(20f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new MoveCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MoveCountRequirement.Event(),
				new MoveCountRequirement.Event(),
				new MoveCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}

namespace Tests.Requirements.Movement.TotalDistance
{
	public sealed class TotalDistanceTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new TotalDistanceTravelledRequirement { RequiredDistance = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void PartialDistance_PartialProgress()
		{
			var req = new TotalDistanceTravelledRequirement { RequiredDistance = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TotalDistanceTravelledRequirement.Event { Distance = 3 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(30f).Within(0.01f));
		}

		[Test]
		public void MultipleEvents_Accumulate()
		{
			var req = new TotalDistanceTravelledRequirement { RequiredDistance = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TotalDistanceTravelledRequirement.Event { Distance = 4 },
				new TotalDistanceTravelledRequirement.Event { Distance = 6 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}

namespace Tests.Requirements.Movement.MaxDistanceInOneMove
{
	public sealed class MaxDistanceInOneMoveTests
	{
		[Test]
		public void BelowThreshold_ZeroProgress()
		{
			var req = new MaxDistanceInOneMoveRequirement { RequiredDistance = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MaxDistanceInOneMoveRequirement.Event { Distance = 3 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void AtThreshold_Completes()
		{
			var req = new MaxDistanceInOneMoveRequirement { RequiredDistance = 4 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MaxDistanceInOneMoveRequirement.Event { Distance = 4 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void TwoSmallMoves_DoNotCombine()
		{
			var req = new MaxDistanceInOneMoveRequirement { RequiredDistance = 4 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MaxDistanceInOneMoveRequirement.Event { Distance = 2 },
				new MaxDistanceInOneMoveRequirement.Event { Distance = 2 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}
