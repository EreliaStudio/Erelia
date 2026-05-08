using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Targeting
{
	// Verifies the three-layer validation boundary:
	//   CanUseAbility        — caster/resource readiness, independent of target cell
	//   CanTargetCellWithAbility — cell geometry (range, LOS, board, profile), no resource check
	//   CanUseAbilityAction  — action-level: source == active unit + resource check via action costs
	public sealed class AbilityValidationBoundaryTests
	{
		// -------------------------------------------------------------------------
		// CanUseAbility — resource layer
		// -------------------------------------------------------------------------

		[Test]
		public void CanUseAbility_ReturnsFalse_WhenInsufficientActionPoints()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 3, range: 10);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			fixture.SetResources(fixture.PlayerUnits[0], actionPoints: 0, movementPoints: 2);

			Assert.That(phase.CanUseAbility(ability), Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void CanUseAbility_ReturnsFalse_WhenInsufficientMovementPoints()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 0, movementPointCost: 3, range: 10);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			fixture.SetResources(fixture.PlayerUnits[0], actionPoints: 2, movementPoints: 1);

			Assert.That(phase.CanUseAbility(ability), Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void CanUseAbility_ReturnsTrue_EvenWhenNoValidTargetExists()
		{
			// Ability with range 1 — enemy is far away, no valid target cells, but resources are fine.
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 1);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			int distance = Mathf.Abs(sourceCell.x - enemyCell.x) + Mathf.Abs(sourceCell.z - enemyCell.z);

			if (distance <= 1)
			{
				Assert.Inconclusive("Enemy is adjacent; test requires distance > 1 to guarantee no valid target.");
			}

			Assert.That(phase.CanUseAbility(ability), Is.True,
				"CanUseAbility should not depend on whether a valid target cell exists.");

			IReadOnlyList<Vector3Int> validCells = phase.GetValidTargetCells(ability);
			Assert.That(validCells.Count, Is.EqualTo(0),
				"GetValidTargetCells should be empty when no enemy is in range.");

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// CanTargetCellWithAbility — cell layer (no resource check)
		// -------------------------------------------------------------------------

		[Test]
		public void CanTargetCellWithAbility_ReturnsFalse_WhenOutOfRange_RegardlessOfResources()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 10);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 1);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			int distance = Mathf.Abs(sourceCell.x - enemyCell.x) + Mathf.Abs(sourceCell.z - enemyCell.z);

			if (distance <= 1)
			{
				Assert.Inconclusive("Enemy is adjacent; need distance > 1 to assert out-of-range.");
			}

			Assert.That(phase.CanTargetCellWithAbility(ability, enemyCell), Is.False,
				"Cell is out of range — should fail regardless of AP.");

			orchestrator.Dispose();
		}

		[Test]
		public void CanTargetCellWithAbility_ReturnsTrue_EvenWhenInsufficientResources()
		{
			// Cell-only check should not care about AP/MP.
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 5, range: 10);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			fixture.SetResources(fixture.PlayerUnits[0], actionPoints: 0, movementPoints: 0);

			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			Assert.That(phase.CanTargetCellWithAbility(ability, enemyCell), Is.True,
				"CanTargetCellWithAbility must not check resources — the cell is geometrically valid.");

			orchestrator.Dispose();
		}

		[Test]
		public void CanTargetCellWithAbility_ReturnsFalse_WhenOutOfBoard()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 100, targetProfile: TargetProfile.Everything);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int outsideCell = new Vector3Int(-999, 1, -999);

			Assert.That(phase.CanTargetCellWithAbility(ability, outsideCell), Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void GetCastLegality_ReturnsInsufficientResources_WhenApMissing_CellOtherwiseValid()
		{
			// GetCastLegality (combined) must still surface InsufficientResources even when the cell is in range.
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 3, range: 10);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			fixture.SetResources(fixture.PlayerUnits[0], actionPoints: 0, movementPoints: 2);

			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);

			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InsufficientResources));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// CanUseAbilityAction — action-level layer
		// -------------------------------------------------------------------------

		[Test]
		public void CanUseAbilityAction_ReturnsFalse_WhenSourceCannotPayActionPoints()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 3, range: 10);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit activeUnit = fixture.PlayerUnits[0];
			fixture.SetResources(activeUnit, actionPoints: 0, movementPoints: 2);

			AbilityAction action = new AbilityAction(activeUnit, ability, new[] { fixture.EnemyUnits[0].BoardPosition });
			bool result = BattleActionValidator.CanUseAbilityAction(fixture.BattleContext, fixture.BattleContext.CurrentTurn, action);

			Assert.That(result, Is.False,
				"CanUseAbilityAction must reject when the unit cannot pay the action's AP cost.");

			orchestrator.Dispose();
		}

		[Test]
		public void CanUseAbilityAction_ReturnsFalse_WhenSourceCannotPayMovementPoints()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 0, movementPointCost: 3, range: 10);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit activeUnit = fixture.PlayerUnits[0];
			fixture.SetResources(activeUnit, actionPoints: 2, movementPoints: 1);

			AbilityAction action = new AbilityAction(activeUnit, ability, new[] { fixture.EnemyUnits[0].BoardPosition });
			bool result = BattleActionValidator.CanUseAbilityAction(fixture.BattleContext, fixture.BattleContext.CurrentTurn, action);

			Assert.That(result, Is.False,
				"CanUseAbilityAction must reject when the unit cannot pay the action's MP cost.");

			orchestrator.Dispose();
		}

		[Test]
		public void CanUseAbilityAction_ReturnsFalse_WhenSourceIsNotActiveUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 10);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			// Action sourced from player[1] while player[0] is active
			BattleUnit nonActiveUnit = fixture.PlayerUnits[1];
			AbilityAction action = new AbilityAction(nonActiveUnit, ability, new[] { fixture.EnemyUnits[0].BoardPosition });
			bool result = BattleActionValidator.CanUseAbilityAction(fixture.BattleContext, fixture.BattleContext.CurrentTurn, action);

			Assert.That(result, Is.False,
				"CanUseAbilityAction must reject when the action source is not the active unit.");

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Resolver rejection when source can no longer pay
		// -------------------------------------------------------------------------

		[Test]
		public void ResolveAbility_ReturnsFalse_WhenSourceDrainedApAfterComposition()
		{
			// Compose a valid action, then drain AP before resolve — resolver must reject.
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 4);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 2, range: 10);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit activeUnit = fixture.PlayerUnits[0];
			AbilityAction action = new AbilityAction(activeUnit, ability, new[] { fixture.EnemyUnits[0].BoardPosition });

			// Drain resources after the action was composed
			fixture.SetResources(activeUnit, actionPoints: 0, movementPoints: 0);

			bool resolved = BattleActionResolver.Resolve(fixture.BattleContext, fixture.BattleContext.CurrentTurn, action);

			Assert.That(resolved, Is.False,
				"Resolver must reject an AbilityAction when the source can no longer pay.");

			orchestrator.Dispose();
		}
	}
}
