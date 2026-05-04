using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Meta
{
	public sealed class MetaRequirementTests
	{
		// ── AndRequirement ────────────────────────────────────────────────────────

		[Test]
		public void And_BothChildrenSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 },
				new HealHealthRequirement.Event { Amount = 5 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void And_OnlyOneChildSatisfied_DoesNotComplete()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
				// No heal event
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void And_ProgressIsMinOfChildren()
		{
			// childA at 50%, childB at 80% → AndRequirement progress = 50%
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new DealDamageRequirement { RequiredAmount = 100 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			// Both use DealDamageRequirement.Event — childA needs 100 total, childB needs 100 total
			// With fight scope on both, 50 damage → both at 50%
			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void And_EmptyChildren_DoesNotComplete()
		{
			var req = new AndRequirement { Children = new List<FeatRequirement>() };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 100 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		// ── OrRequirement ─────────────────────────────────────────────────────────

		[Test]
		public void Or_FirstChildSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Or_SecondChildSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new HealHealthRequirement.Event { Amount = 5 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Or_NeitherChildSatisfied_DoesNotComplete()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new HealHealthRequirement { RequiredAmount = 100 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 40 },
				new HealHealthRequirement.Event { Amount = 30 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void Or_ProgressIsMaxOfChildren()
		{
			// childA at 40%, childB at 80% → OrRequirement progress = 80%
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new HealHealthRequirement { RequiredAmount = 100 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 40 },
				new HealHealthRequirement.Event { Amount = 80 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(80f).Within(0.01f));
		}
	}
}
