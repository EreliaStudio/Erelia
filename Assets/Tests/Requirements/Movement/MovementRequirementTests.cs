using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Movement
{
	public sealed class MovementRequirementTests
	{
		// ── MoveCountRequirement ──────────────────────────────────────────────────

		[Test]
		public void MoveCount_NoEvents_ZeroProgress()
		{
			var req = new MoveCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void MoveCount_OneMove_PartialProgress()
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
		public void MoveCount_ReachingRequired_Completes()
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

		// ── TotalDistanceTravelledRequirement ─────────────────────────────────────

		[Test]
		public void TotalDistance_NoEvents_ZeroProgress()
		{
			var req = new TotalDistanceTravelledRequirement { RequiredDistance = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void TotalDistance_PartialDistance_PartialProgress()
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
		public void TotalDistance_MultipleEvents_Accumulate()
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

		// ── MaxDistanceInOneMoveRequirement ───────────────────────────────────────

		[Test]
		public void MaxDistanceInOneMove_BelowThreshold_ZeroProgress()
		{
			var req = new MaxDistanceInOneMoveRequirement { RequiredDistance = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			// Ability scope: each event evaluated independently
			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new MaxDistanceInOneMoveRequirement.Event { Distance = 3 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void MaxDistanceInOneMove_AtThreshold_Completes()
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
		public void MaxDistanceInOneMove_TwoSmallMoves_DoNotCombine()
		{
			// Ability scope — two moves of distance 2 must NOT combine to reach threshold of 4
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
