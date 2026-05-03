using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.FightScope
{
	public sealed class FightScopeTests
	{
		[Test]
		public void TreatsWholeFightAsOneWindow()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Fight,
					RequiredRepeatCount = 2
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 80, TurnIndex = 1 },
				new TestAmountEvent { Amount = 80, TurnIndex = 2 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
			Assert.That(progress.IsCompleted, Is.False);
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
