using NUnit.Framework;

public sealed class BattleCombatFlowTests
{
	[Test]
	public void WholeCombat_PlayerKillsEnemy_AndBattleEndsWithPlayerVictory()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1, defaultMovement: 0);
		Ability killingAbility = fixture.CreateDamageAbility(baseDamage: 99, actionPointCost: 1, targetProfile: TargetProfile.Enemy, requireLineOfSight: false);
		fixture.PlayerSources[0].Abilities.Add(killingAbility);

		BattleOutcome emittedOutcome = null;
		EventCenter.BattleEnded += OnBattleEnded;

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
			Assert.That(emittedOutcome, Is.Not.Null);
			Assert.That(emittedOutcome.Winner, Is.EqualTo(BattleSide.Player));
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
