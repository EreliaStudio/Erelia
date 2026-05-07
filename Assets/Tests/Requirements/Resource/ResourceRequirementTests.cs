using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Resource.ConsumeActionPoints
{
	public sealed class ConsumeActionPointsTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void PartialAmount_PartialProgress()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, Amount = 4 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(40f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, RequiredAmount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, Amount = 5 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void MultipleEvents_Accumulate()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, RequiredAmount = 6 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, Amount = 2 },
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, Amount = 4 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void WrongResourceKind_IsIgnored()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, RequiredAmount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, Amount = 10 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}
	}
}

namespace Tests.Requirements.Resource.ConsumeMovementPoints
{
	public sealed class ConsumeMovementPointsTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, RequiredAmount = 10 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void PartialAmount_PartialProgress()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, RequiredAmount = 8 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, Amount = 4 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, RequiredAmount = 6 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, Amount = 3 },
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, Amount = 3 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void WrongResourceKind_IsIgnored()
		{
			var req = new ConsumeResourcesRequirement { RequiredResource = ConsumeResourcesRequirement.ResourceKind.MovementPoints, RequiredAmount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ConsumeResourcesRequirement.Event { Resource = ConsumeResourcesRequirement.ResourceKind.ActionPoints, Amount = 10 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}
	}
}
