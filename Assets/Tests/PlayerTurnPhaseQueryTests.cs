using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class PlayerTurnPhaseQueryTests
{
	[Test]
	public void CanMoveTo_ReturnsTrueForReachableDestination()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out _, out _);
		Vector3Int destination = playerTurnPhase.GetReachableCells()[0];

		Assert.That(playerTurnPhase.CanMoveTo(destination), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void GetReachableCells_ReturnsReachableDestinations()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out _, out _);

		Assert.That(playerTurnPhase.GetReachableCells().Count, Is.GreaterThan(0));

		orchestrator.Dispose();
	}

	[Test]
	public void CanUseAbility_ReturnsTrueWhenEnemyIsInRange()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out _);

		Assert.That(playerTurnPhase.CanUseAbility(ability), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void GetCastLegality_ReturnsValidForEnemyCell()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		Assert.That(playerTurnPhase.GetCastLegality(ability, enemyCell).IsValid, Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void GetValidTargets_ReturnsEnemyUnit()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out _);

		CollectionAssert.Contains(playerTurnPhase.GetValidTargets(ability), fixture.EnemyUnits[0]);

		orchestrator.Dispose();
	}

	[Test]
	public void GetValidTargetCells_ReturnsEnemyCell()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		CollectionAssert.Contains(playerTurnPhase.GetValidTargetCells(ability), enemyCell);

		orchestrator.Dispose();
	}

	[Test]
	public void GetAffectedCells_ReturnsAnchorCellForSingleTargetAbility()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		CollectionAssert.AreEquivalent(new[] { enemyCell }, playerTurnPhase.GetAffectedCells(ability, enemyCell));

		orchestrator.Dispose();
	}

	[Test]
	public void GetAffectedObjects_ReturnsEnemyObjectAtAnchorCell()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		CollectionAssert.Contains(playerTurnPhase.GetAffectedObjects(ability, enemyCell), fixture.EnemyUnits[0]);

		orchestrator.Dispose();
	}

	[Test]
	public void CanTarget_ReturnsTrueForEnemyUnit()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out _);

		Assert.That(playerTurnPhase.CanTarget(ability, fixture.EnemyUnits[0]), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CanTargetCell_ReturnsTrueForEnemyCell()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		Assert.That(playerTurnPhase.CanTargetCell(ability, enemyCell), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CanCastAtCell_ReturnsTrueForEnemyCell()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out Ability ability, out Vector3Int enemyCell);

		Assert.That(playerTurnPhase.CanCastAtCell(ability, enemyCell), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CanEndTurn_ReturnsTrueWithoutPendingAction()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out _, out _);

		Assert.That(playerTurnPhase.CanEndTurn(), Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void TryGetPathTo_ReturnsAPathToReachableDestination()
	{
		using BattlePhaseTestFixture fixture = CreatePlayerTurnFixture(out BattleOrchestrator orchestrator, out PlayerTurnPhase playerTurnPhase, out _, out _);
		Vector3Int destination = playerTurnPhase.GetReachableCells()[0];

		Assert.That(playerTurnPhase.TryGetPathTo(destination, out IReadOnlyList<Vector3Int> path), Is.True);
		Assert.That(path.Count, Is.GreaterThan(0));
		Assert.That(path[path.Count - 1], Is.EqualTo(destination));

		orchestrator.Dispose();
	}

	private static BattlePhaseTestFixture CreatePlayerTurnFixture(
		out BattleOrchestrator orchestrator,
		out PlayerTurnPhase playerTurnPhase,
		out Ability ability,
		out Vector3Int enemyCell)
	{
		BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		ability = fixture.CreateDamageAbility(baseDamage: 2, actionPointCost: 1, targetProfile: TargetProfile.Enemy, requireLineOfSight: false);
		fixture.PlayerSources[0].Abilities.Add(ability);

		orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
		enemyCell = fixture.EnemyUnits[0].BoardPosition;
		return fixture;
	}
}
