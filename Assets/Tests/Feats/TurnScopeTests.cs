using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.TurnScope
{
	public sealed class TurnScopeTests
	{
		[Test]
		public void EventsInSameTurnCombine()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Turn
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 60, TurnIndex = 1 },
				new TestAmountEvent { Amount = 50, TurnIndex = 1 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void EventsInDifferentTurnsDoNotCombine()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Turn
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 60, TurnIndex = 1 },
				new TestAmountEvent { Amount = 60, TurnIndex = 2 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
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
