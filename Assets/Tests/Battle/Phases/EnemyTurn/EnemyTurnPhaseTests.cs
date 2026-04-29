using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Phases.EnemyTurn
{
	public sealed class EnemyTurnPhaseTests
	{
		[Test]
		public void Enter_AttacksPlayerWhenEnemyCanUseAnAbility()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1, defaultMovement: 0);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 3, actionPointCost: 1, targetProfile: TargetProfile.Enemy, requireLineOfSight: false);
			fixture.EnemySources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);
			fixture.PlaceAllPlayers(placementPhase);
			fixture.SetTurnBars(
				playerTurnBars: new[] { 0f },
				enemyTurnBars: new[] { fixture.EnemyUnits[0].BattleAttributes.TurnBar.Max });

			int playerHealthBefore = fixture.PlayerUnits[0].BattleAttributes.Health.Current;
			Assert.That(placementPhase.TryCompletePlacement(), Is.True);
			Assert.That(fixture.PlayerUnits[0].BattleAttributes.Health.Current, Is.LessThan(playerHealthBefore));

			orchestrator.Dispose();
		}

		[Test]
		public void Enter_MovesTowardPlayerWhenEnemyCannotAttack()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 0, defaultMovement: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);
			fixture.PlaceAllPlayers(placementPhase);
			fixture.SetTurnBars(
				playerTurnBars: new[] { 0f },
				enemyTurnBars: new[] { fixture.EnemyUnits[0].BattleAttributes.TurnBar.Max });

			Vector3Int initialEnemyPosition = fixture.EnemyUnits[0].BoardPosition;
			int initialDistance = ManhattanDistance(initialEnemyPosition, fixture.PlayerUnits[0].BoardPosition);

			Assert.That(placementPhase.TryCompletePlacement(), Is.True);

			int movedDistance = ManhattanDistance(fixture.EnemyUnits[0].BoardPosition, fixture.PlayerUnits[0].BoardPosition);
			Assert.That(movedDistance, Is.LessThan(initialDistance));

			orchestrator.Dispose();
		}

		[Test]
		public void Enter_EndsTurnWhenEnemyCannotAttackOrMove()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 0, defaultMovement: 0);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);
			fixture.PlaceAllPlayers(placementPhase);
			fixture.SetTurnBars(
				playerTurnBars: new[] { 0f },
				enemyTurnBars: new[] { fixture.EnemyUnits[0].BattleAttributes.TurnBar.Max });

			Assert.That(placementPhase.TryCompletePlacement(), Is.True);
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));

			orchestrator.Dispose();
		}

		private static int ManhattanDistance(Vector3Int a, Vector3Int b)
		{
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
		}
	}
}
