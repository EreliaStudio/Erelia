using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Phases.Resolution
{
	public sealed class ResolutionPhaseTests
	{
		[Test]
		public void Enter_ReturnsToCurrentTurnPhaseWhenNoPendingActionExists()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			orchestrator.TransitionTo(BattlePhaseType.Resolution);

			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));

			orchestrator.Dispose();
		}

		[Test]
		public void Enter_ResolvesPendingMoveAction()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 2);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = playerTurnPhase.GetReachableCells()[0];

			Assert.That(orchestrator.TurnContext.TrySetPendingAction(new MoveAction(fixture.PlayerUnits[0], destination)), Is.True);
			orchestrator.TransitionTo(BattlePhaseType.Resolution);

			Assert.That(fixture.PlayerUnits[0].BoardPosition, Is.EqualTo(destination));
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));

			orchestrator.Dispose();
		}
	}
}
