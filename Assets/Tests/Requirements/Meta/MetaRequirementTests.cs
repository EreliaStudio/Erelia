using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Meta.And
{
	public sealed class AndTests
	{
		[Test]
		public void BothChildrenSatisfied_Completes()
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
		public void OnlyOneChildSatisfied_DoesNotComplete()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void ProgressIsMinOfChildren()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new DealDamageRequirement { RequiredAmount = 100 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 50 }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void EmptyChildren_DoesNotComplete()
		{
			var req = new AndRequirement { Children = new List<FeatRequirement>() };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 100 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Meta.Or
{
	public sealed class OrTests
	{
		[Test]
		public void FirstChildSatisfied_Completes()
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
		public void SecondChildSatisfied_Completes()
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
		public void NeitherChildSatisfied_DoesNotComplete()
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
		public void ProgressIsMaxOfChildren()
		{
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
