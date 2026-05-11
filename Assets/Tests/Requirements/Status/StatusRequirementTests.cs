using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Status.ApplyStatusCount
{
	public sealed class ApplyStatusCountTests
	{
		private static BattleUnit CreateBattleUnit()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
			return new BattleUnit(new CreatureUnit { Species = species }, BattleSide.Player);
		}

		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneEvent_PartialProgress()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 5 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Caster = CreateBattleUnit() }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(20f).Within(0.01f));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new ApplyStatusCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Caster = CreateBattleUnit() },
				new StatusAppliedEvent { Caster = CreateBattleUnit() },
				new StatusAppliedEvent { Caster = CreateBattleUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void NullStatusFilter_AnyStatusCounts()
		{
			global::Status status = ScriptableObject.CreateInstance<global::Status>();
			var req = new ApplyStatusCountRequirement { RequiredStatus = null, RequiredCount = 1 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Caster = CreateBattleUnit(), Status = status }
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

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Caster = CreateBattleUnit(), Status = status }
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

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Status = statusB }
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

			progress.RegisterEvents(new List<BattleEvent>
			{
				new StatusAppliedEvent { Caster = CreateBattleUnit(), Status = status },
				new StatusAppliedEvent { Caster = CreateBattleUnit(), Status = status }
			});

			Assert.That(progress.IsCompleted, Is.True);
			Object.DestroyImmediate(status);
		}
	}
}
