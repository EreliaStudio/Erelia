using NUnit.Framework;

namespace Tests.Battle.Phases.End
{
	public sealed class EndPhaseTests
	{
		[Test]
		public void Enter_EmitsBattleResolvedOutcome()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.PlayerUnits[0].BattleAttributes.Health.SetCurrent(5, true);
			fixture.EnemyUnits[0].BattleAttributes.Health.SetCurrent(0, true);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			BattleSide? emittedWinner = null;
			EventCenter.BattleResolved += OnBattleResolved;

			try
			{
				orchestrator.TransitionTo(BattlePhaseType.End);

				Assert.That(emittedWinner, Is.Not.Null);
				Assert.That(emittedWinner, Is.EqualTo(BattleSide.Player));
			}
			finally
			{
				EventCenter.BattleResolved -= OnBattleResolved;
				orchestrator.Dispose();
			}

			void OnBattleResolved(BattleContext context, BattleSide winner)
			{
				emittedWinner = winner;
			}
		}
	}
}
