using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Phases.PlayerTurn
{
	public sealed class PlayerTurnPhaseCommandTests
	{
		[Test]
		public void TrySubmitMove_ResolvesMovementAndKeepsSameActiveUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 2);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int start = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int destination = playerTurnPhase.GetReachableCells()[0];

			Assert.That(playerTurnPhase.TrySubmitMove(destination), Is.True);
			Assert.That(fixture.PlayerUnits[0].BoardPosition, Is.EqualTo(destination));
			Assert.That(fixture.PlayerUnits[0].BattleAttributes.MovementPoints.Current, Is.LessThan(2));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(destination, Is.Not.EqualTo(start));

			orchestrator.Dispose();
		}

		[Test]
		public void TrySubmitAbility_ResolvesDamageAgainstEnemyAndKeepsTurnWhenMovementRemains()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1, defaultMovement: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 3, actionPointCost: 1, targetProfile: TargetProfile.Enemy, requireLineOfSight: false);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
			int enemyHealthBefore = fixture.EnemyUnits[0].BattleAttributes.Health.Current;

			Assert.That(playerTurnPhase.TrySubmitAbility(ability, new[] { fixture.EnemyUnits[0].BoardPosition }), Is.True);
			Assert.That(fixture.EnemyUnits[0].BattleAttributes.Health.Current, Is.LessThan(enemyHealthBefore));
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));

			orchestrator.Dispose();
		}

		[Test]
		public void TrySubmitEndTurn_AdvancesToNextReadyPlayerUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1, defaultActionPoints: 1, defaultMovement: 0);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);

			Assert.That(playerTurnPhase.TrySubmitEndTurn(), Is.True);
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[1]));

			orchestrator.Dispose();
		}
	}
}
