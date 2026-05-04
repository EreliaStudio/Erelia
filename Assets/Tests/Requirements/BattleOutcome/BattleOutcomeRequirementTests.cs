using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.BattleOutcome.WinBattle
{
	public sealed class WinBattleTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
		}

		[Test]
		public void OneWinEvent_OneCompletion()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event()
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
		}

		[Test]
		public void MultipleWinEvents_AccumulatesCompletions()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase> { new WinBattleCountRequirement.Event() });
			progress.RegisterEvents(new List<FeatRequirement.EventBase> { new WinBattleCountRequirement.Event() });
			progress.RegisterEvents(new List<FeatRequirement.EventBase> { new WinBattleCountRequirement.Event() });

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}

namespace Tests.Requirements.BattleOutcome.SurviveBattle
{
	public sealed class SurviveBattleTests
	{
		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 2, RequireUnitSurvival = true };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
		}

		[Test]
		public void UnitSurvived_OneCompletion()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 2, RequireUnitSurvival = true };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event { UnitSurvived = true }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
		}

		[Test]
		public void UnitLost_ZeroProgress()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 2, RequireUnitSurvival = true };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event { UnitSurvived = false }
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
		}

		[Test]
		public void ReachingRequired_Completes()
		{
			var req = new WinBattleCountRequirement { RequiredRepeatCount = 2, RequireUnitSurvival = true };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase> { new WinBattleCountRequirement.Event { UnitSurvived = true } });
			progress.RegisterEvents(new List<FeatRequirement.EventBase> { new WinBattleCountRequirement.Event { UnitSurvived = true } });

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
