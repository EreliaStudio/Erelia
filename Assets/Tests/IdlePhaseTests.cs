using NUnit.Framework;

public sealed class IdlePhaseTests
{
	[Test]
	public void Enter_BeginsTheNextReadyTurn()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);

		fixture.PlaceAllPlayers(placementPhase);
		fixture.SetTurnBars(
			playerTurnBars: new[] { 0f, fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		orchestrator.TransitionTo(BattlePhaseType.Idle);

		Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
		Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[1]));
	}
}
