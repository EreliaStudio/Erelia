using NUnit.Framework;

namespace Tests.Battle.Flow
{
	public sealed class BattleCombatFlowTests
	{
		[Test]
		public void WholeCombat_PlayerKillsEnemy_AndBattleEndsWithPlayerVictory()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1, defaultMovement: 0);
			Ability killingAbility = fixture.CreateDamageAbility(baseDamage: 99, actionPointCost: 1, targetProfile: TargetProfile.Enemy, requireLineOfSight: false);
			fixture.PlayerSources[0].Abilities.Add(killingAbility);

			BattleSide? emittedWinner = null;
			EventCenter.BattleResolved += OnBattleResolved;

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();

			try
			{
				fixture.CompletePlacement(
					orchestrator,
					playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
					enemyTurnBars: new[] { 0f });

				PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
				Assert.That(playerTurnPhase.TrySubmitAbility(killingAbility, new[] { fixture.EnemyUnits[0].BoardPosition }), Is.True);

				Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.End));
				Assert.That(fixture.EnemyUnits[0].IsDefeated, Is.True);
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
