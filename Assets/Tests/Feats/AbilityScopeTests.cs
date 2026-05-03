using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.AbilityScope
{
	public sealed class AbilityScopeTests
	{
		[Test]
		public void EventBelowThresholdDoesNotComplete()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Ability
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 30 },
				new TestAmountEvent { Amount = 40 },
				new TestAmountEvent { Amount = 60 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void EventAtThresholdCompletes()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Ability
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 60 },
				new TestAmountEvent { Amount = 100 },
				new TestAmountEvent { Amount = 20 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void CountsEachQualifyingEventSeparately()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Ability,
					RequiredRepeatCount = 2
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 100 },
				new TestAmountEvent { Amount = 20 },
				new TestAmountEvent { Amount = 100 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(2));
			Assert.That(progress.IsCompleted, Is.True);
		}

		private sealed class TestAmountRequirement : FeatRequirementTemplated<TestAmountEvent>
		{
			public int RequiredAmount = 100;

			protected override float EvaluateProgress(TestAmountEvent p_event)
			{
				return ComputeLinearProgress(p_event.Amount, RequiredAmount);
			}
		}

		[Serializable]
		private sealed class TestAmountEvent : FeatRequirement.EventBase
		{
			public int Amount;
		}
	}
}
