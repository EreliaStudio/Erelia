using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.GameScope
{
	public sealed class GameScopeTests
	{
		[Test]
		public void CarriesPartialProgressAcrossEvaluations()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new TestAmountRequirement
				{
					RequiredAmount = 100,
					RequirementScope = FeatRequirement.Scope.Game
				}
			};

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 60, TurnIndex = 1 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.CurrentProgress, Is.EqualTo(60f).Within(0.01f));

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TestAmountEvent { Amount = 40, TurnIndex = 2 }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
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
