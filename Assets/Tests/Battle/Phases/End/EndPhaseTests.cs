using NUnit.Framework;

namespace Tests.Battle.Phases.End
{
	public sealed class EndPhaseTests
	{
		[Test]
		public void Enter_EmitsBattleEndedOutcome()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.PlayerUnits[0].BattleAttributes.Health.SetCurrent(5, true);
			fixture.EnemyUnits[0].BattleAttributes.Health.SetCurrent(0, true);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			BattleOutcome emittedOutcome = null;
			EventCenter.BattleEnded += OnBattleEnded;

			try
			{
				orchestrator.TransitionTo(BattlePhaseType.End);

				Assert.That(emittedOutcome, Is.Not.Null);
				Assert.That(emittedOutcome.Winner, Is.EqualTo(BattleSide.Player));
				Assert.That(emittedOutcome.SurvivingPlayerUnits.Count, Is.EqualTo(1));
				Assert.That(emittedOutcome.SurvivingEnemyUnits.Count, Is.EqualTo(0));
			}
			finally
			{
				EventCenter.BattleEnded -= OnBattleEnded;
				orchestrator.Dispose();
			}

			void OnBattleEnded(BattleOutcome outcome)
			{
				emittedOutcome = outcome;
			}
		}
	}
}
