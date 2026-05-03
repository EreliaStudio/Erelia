using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.BattleOutcome
{
	public sealed class BattleOutcomeRequirementTests
	{
		// ── WinBattleCountRequirement ─────────────────────────────────────────────

		[Test]
		public void WinBattle_NoEvents_ZeroProgress()
		{
			var req = new WinBattleCountRequirement();
			req.RequiredRepeatCount = 3;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
		}

		[Test]
		public void WinBattle_OneWinEvent_OneCompletion()
		{
			var req = new WinBattleCountRequirement();
			req.RequiredRepeatCount = 3;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event()
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
		}

		[Test]
		public void WinBattle_MultipleWinEvents_AccumulatesCompletions()
		{
			var req = new WinBattleCountRequirement();
			req.RequiredRepeatCount = 3;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event()
			});
			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event()
			});
			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new WinBattleCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		// ── SurviveBattleCountRequirement ─────────────────────────────────────────

		[Test]
		public void SurviveBattle_NoEvents_ZeroProgress()
		{
			var req = new SurviveBattleCountRequirement();
			req.RequiredRepeatCount = 2;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>());

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
		}

		[Test]
		public void SurviveBattle_OneSurviveEvent_OneCompletion()
		{
			var req = new SurviveBattleCountRequirement();
			req.RequiredRepeatCount = 2;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveBattleCountRequirement.Event()
			});

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(1));
		}

		[Test]
		public void SurviveBattle_ReachingRequired_Completes()
		{
			var req = new SurviveBattleCountRequirement();
			req.RequiredRepeatCount = 2;
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveBattleCountRequirement.Event()
			});
			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new SurviveBattleCountRequirement.Event()
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
