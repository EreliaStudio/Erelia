using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Status.ApplyStatusCount
{
	public sealed class ApplyStatusCountTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneEvent_PartialProgress()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event()
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(20f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event(),
				new ApplyStatusCountRequirement.Event(),
				new ApplyStatusCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void NullStatusFilter_AnyStatusCounts()
		{
			global::Status status = ScriptableObject.CreateInstance<global::Status>();
			var req = new ApplyStatusCountRequirement { RequiredStatus = null, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event { Status = status }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(status);
		}

		[Test]
		public void MatchingStatus_CountsProgress()
		{
			global::Status status = ScriptableObject.CreateInstance<global::Status>();
			var req = new ApplyStatusCountRequirement { RequiredStatus = status, RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event { Status = status }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
			Object.DestroyImmediate(status);
		}

		[Test]
		public void WrongStatus_ZeroProgress()
		{
			global::Status statusA = ScriptableObject.CreateInstance<global::Status>();
			global::Status statusB = ScriptableObject.CreateInstance<global::Status>();
			var req = new ApplyStatusCountRequirement { RequiredStatus = statusA, RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event { Status = statusB }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
			Object.DestroyImmediate(statusA);
			Object.DestroyImmediate(statusB);
		}

		[Test]
		public void MatchingStatusReachesRequired_Completes()
		{
			global::Status status = ScriptableObject.CreateInstance<global::Status>();
			var req = new ApplyStatusCountRequirement { RequiredStatus = status, RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new ApplyStatusCountRequirement.Event { Status = status },
				new ApplyStatusCountRequirement.Event { Status = status }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(status);
		}
	}
}
