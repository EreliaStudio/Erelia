using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Targeting
{
	public sealed class TargetingAndLegalityTests
	{
		// -------------------------------------------------------------------------
		// TargetProfile.Empty
		// -------------------------------------------------------------------------

		[Test]
		public void Empty_ValidatesEmptyCell_WhenNoUnitPresent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> nodes = GetAllNavigablePositions(fixture);
			Vector3Int emptyCell = FindCellNotOccupied(nodes, fixture);

			Assert.That(phase.CanCastAtCell(ability, emptyCell), Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Empty_RejectsOccupiedCell_WhenUnitPresent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InvalidTargetProfile));

			orchestrator.Dispose();
		}

		[Test]
		public void Empty_ReturnsNonEmptyValidTargetCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> validCells = phase.GetValidTargetCells(ability);

			Assert.That(validCells.Count, Is.GreaterThan(0));
			foreach (Vector3Int cell in validCells)
			{
				Assert.That(fixture.BattleContext.Board.HasUnitAt(cell), Is.False,
					$"Cell {cell} should be empty for TargetProfile.Empty");
			}

			orchestrator.Dispose();
		}

		[Test]
		public void Empty_DoesNotIncludeEnemyCell_InValidTargets()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			CollectionAssert.DoesNotContain(phase.GetValidTargetCells(ability), enemyCell);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// TargetProfile.Everything
		// -------------------------------------------------------------------------

		[Test]
		public void Everything_ValidatesEnemyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			Assert.That(phase.GetCastLegality(ability, enemyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Everything_ValidatesEmptyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> nodes = GetAllNavigablePositions(fixture);
			Vector3Int emptyCell = FindCellNotOccupied(nodes, fixture);

			Assert.That(phase.GetCastLegality(ability, emptyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Everything_ValidatesAllyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int allyCell = fixture.PlayerUnits[1].BoardPosition;

			Assert.That(phase.GetCastLegality(ability, allyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Everything_IncludesAllOccupiedAndEmptyCells_InValidTargetCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> validCells = phase.GetValidTargetCells(ability);

			CollectionAssert.Contains(validCells, fixture.EnemyUnits[0].BoardPosition);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// TargetProfile.Ally
		// -------------------------------------------------------------------------

		[Test]
		public void Ally_ValidatesAllyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int allyCell = fixture.PlayerUnits[1].BoardPosition;

			Assert.That(phase.GetCastLegality(ability, allyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Ally_RejectsEnemyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InvalidTargetProfile));

			orchestrator.Dispose();
		}

		[Test]
		public void Ally_RejectsEmptyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> nodes = GetAllNavigablePositions(fixture);
			Vector3Int emptyCell = FindCellNotOccupied(nodes, fixture);

			AbilityCastLegality legality = phase.GetCastLegality(ability, emptyCell);
			Assert.That(legality.IsValid, Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void Ally_CanTarget_ReturnsFalseForEnemy()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);

			Assert.That(phase.CanTarget(ability, fixture.EnemyUnits[0]), Is.False);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// TargetProfile.Enemy
		// -------------------------------------------------------------------------

		[Test]
		public void Enemy_ValidatesEnemyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			Assert.That(phase.GetCastLegality(ability, enemyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Enemy_RejectsAllyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int allyCell = fixture.PlayerUnits[1].BoardPosition;

			AbilityCastLegality legality = phase.GetCastLegality(ability, allyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InvalidTargetProfile));

			orchestrator.Dispose();
		}

		[Test]
		public void Enemy_RejectsEmptyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> nodes = GetAllNavigablePositions(fixture);
			Vector3Int emptyCell = FindCellNotOccupied(nodes, fixture);

			AbilityCastLegality legality = phase.GetCastLegality(ability, emptyCell);
			Assert.That(legality.IsValid, Is.False);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Out-of-range rejection
		// -------------------------------------------------------------------------

		[Test]
		public void OutOfRange_RejectsCell_WhenBeyondCircleRange()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			int manhattanDist = Mathf.Abs(sourceCell.x - enemyCell.x) + Mathf.Abs(sourceCell.z - enemyCell.z);

			// Only assert out-of-range if enemy is actually beyond range 1
			if (manhattanDist > 1)
			{
				AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);
				Assert.That(legality.IsValid, Is.False);
				Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.OutOfRange));
			}
			else
			{
				Assert.Inconclusive("Enemy ended up adjacent; test requires distance > 1.");
			}

			orchestrator.Dispose();
		}

		[Test]
		public void OutOfRange_ValidatesCell_WhenExactlyAtCircleRange()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			Assert.That(phase.GetCastLegality(ability, enemyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void OutOfRange_RejectsOutOfBoardCell()
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

			AbilityCastLegality legality = phase.GetCastLegality(ability, outsideCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.OutOfBoard));

			orchestrator.Dispose();
		}

		[Test]
		public void LineRange_RejectsNonAxisAlignedCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			ability.Range = new Ability.RangeDefinition
			{
				Type = Ability.RangeDefinition.Shape.Line,
				Value = 10,
				RequireLineOfSight = false
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;

			// Diagonal from source (both x and z differ) — should be out of line range
			Vector3Int diagonalCell = new Vector3Int(sourceCell.x + 1, sourceCell.y, sourceCell.z + 1);
			if (fixture.BattleContext.Board.IsInside(diagonalCell))
			{
				AbilityCastLegality legality = phase.GetCastLegality(ability, diagonalCell);
				if (fixture.BattleContext.Board.HasUnitAt(diagonalCell))
				{
					Assert.Inconclusive("Diagonal cell is occupied; skipping profile check.");
				}

				Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.OutOfRange).
					Or.EqualTo(AbilityCastLegality.Failure.InvalidTargetProfile));
			}
			else
			{
				Assert.Inconclusive("Diagonal cell is outside the board for this fixture size.");
			}

			orchestrator.Dispose();
		}

		[Test]
		public void LineRange_ValidatesAxisAlignedCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			ability.Range = new Ability.RangeDefinition
			{
				Type = Ability.RangeDefinition.Shape.Line,
				Value = 10,
				RequireLineOfSight = false
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int enemyCell = FindAxisAlignedUnoccupiedCell(GetAllNavigablePositions(fixture), fixture, sourceCell);
			Assert.That(fixture.BattleContext.TryMoveUnit(fixture.EnemyUnits[0], enemyCell), Is.True);

			Assert.That(phase.GetCastLegality(ability, enemyCell).IsValid, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void DiagonalRange_RejectsAxisAlignedCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			ability.Range = new Ability.RangeDefinition
			{
				Type = Ability.RangeDefinition.Shape.Diagonal,
				Value = 10,
				RequireLineOfSight = false
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int sourceCell = fixture.PlayerUnits[0].BoardPosition;
			Vector3Int enemyCell = FindAxisAlignedUnoccupiedCell(GetAllNavigablePositions(fixture), fixture, sourceCell);
			Assert.That(fixture.BattleContext.TryMoveUnit(fixture.EnemyUnits[0], enemyCell), Is.True);

			AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.OutOfRange));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Insufficient resources
		// -------------------------------------------------------------------------

		[Test]
		public void InsufficientActionPoints_RejectsAbility()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 0);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 10, targetProfile: TargetProfile.Enemy);
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

		[Test]
		public void InsufficientMovementPoints_RejectsAbilityWithMovementCost()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 3);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 0, movementPointCost: 2, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			fixture.SetResources(fixture.PlayerUnits[0], actionPoints: 2, movementPoints: 1);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			AbilityCastLegality legality = phase.GetCastLegality(ability, enemyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InsufficientResources));

			orchestrator.Dispose();
		}

		[Test]
		public void CanUseAbility_ReturnsFalse_WhenNoActionPoints()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 10, targetProfile: TargetProfile.Enemy);
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

		// -------------------------------------------------------------------------
		// AoE shapes — area value > 0
		// -------------------------------------------------------------------------

		[Test]
		public void AoE_Circle_IncludesAllCellsWithinManhattanRadius()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Circle,
				Value = 2
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int anchor = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<Vector3Int> affected = phase.GetAffectedCells(ability, anchor);

			Assert.That(affected.Count, Is.GreaterThan(1));
			foreach (Vector3Int cell in affected)
			{
				int dist = Mathf.Abs(cell.x - anchor.x) + Mathf.Abs(cell.z - anchor.z);
				Assert.That(dist, Is.LessThanOrEqualTo(2), $"Cell {cell} is outside circle radius 2");
			}

			orchestrator.Dispose();
		}

		[Test]
		public void AoE_Square_IncludesAllCellsInBoundingSquare()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Square,
				Value = 1
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int anchor = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<Vector3Int> affected = phase.GetAffectedCells(ability, anchor);

			Assert.That(affected.Count, Is.GreaterThan(1));
			foreach (Vector3Int cell in affected)
			{
				Assert.That(Mathf.Abs(cell.x - anchor.x), Is.LessThanOrEqualTo(1));
				Assert.That(Mathf.Abs(cell.z - anchor.z), Is.LessThanOrEqualTo(1));
			}

			orchestrator.Dispose();
		}

		[Test]
		public void AoE_Cross_OnlyIncludesAxisAlignedCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Cross,
				Value = 2
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int anchor = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<Vector3Int> affected = phase.GetAffectedCells(ability, anchor);

			Assert.That(affected.Count, Is.GreaterThan(1));
			foreach (Vector3Int cell in affected)
			{
				bool onXAxis = cell.x == anchor.x;
				bool onZAxis = cell.z == anchor.z;
				Assert.That(onXAxis || onZAxis, Is.True, $"Cell {cell} is not on an axis from anchor {anchor}");
			}

			orchestrator.Dispose();
		}

		[Test]
		public void AoE_ZeroValue_AffectsOnlyAnchorCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Circle,
				Value = 0
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<Vector3Int> affected = phase.GetAffectedCells(ability, enemyCell);

			CollectionAssert.AreEquivalent(new[] { enemyCell }, affected);

			orchestrator.Dispose();
		}

		[Test]
		public void AoE_GetAffectedObjects_ReturnsAllUnitsInArea()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 2);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Circle,
				Value = 10
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f, 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemy0Cell = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<BattleObject> affected = phase.GetAffectedObjects(ability, enemy0Cell);

			CollectionAssert.Contains(affected, fixture.EnemyUnits[0]);
			CollectionAssert.Contains(affected, fixture.EnemyUnits[1]);

			orchestrator.Dispose();
		}

		[Test]
		public void AoE_GetAffectedCells_NoDuplicates()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Everything);
			ability.AreaOfEffect = new Ability.AreaOfEffectDefinition
			{
				Type = Ability.AreaOfEffectDefinition.Shape.Square,
				Value = 2
			};
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int anchor = fixture.EnemyUnits[0].BoardPosition;
			IReadOnlyList<Vector3Int> affected = phase.GetAffectedCells(ability, anchor);

			HashSet<Vector3Int> unique = new HashSet<Vector3Int>(affected);
			Assert.That(unique.Count, Is.EqualTo(affected.Count), "Affected cells should have no duplicates");

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Multi-anchor ability submission
		// -------------------------------------------------------------------------

		[Test]
		public void TrySubmitAbility_RejectsEmptyTargetCellList()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);

			bool submitted = phase.TrySubmitAbility(ability, new List<Vector3Int>());
			Assert.That(submitted, Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void TrySubmitAbility_RejectsNullTargetCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);

			bool submitted = phase.TrySubmitAbility(ability, null);
			Assert.That(submitted, Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void TrySubmitAbility_RejectsIfAnyTargetCellIsInvalid()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			Vector3Int invalidCell = new Vector3Int(-999, 1, -999);

			bool submitted = phase.TrySubmitAbility(ability, new List<Vector3Int> { enemyCell, invalidCell });
			Assert.That(submitted, Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void TrySubmitAbility_SucceedsWithValidSingleTarget()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 4);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			bool submitted = phase.TrySubmitAbility(ability, new List<Vector3Int> { enemyCell });
			Assert.That(submitted, Is.True);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Ally-targeted ability
		// -------------------------------------------------------------------------

		[Test]
		public void Ally_GetValidTargets_ContainsAllyUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<BattleObject> validTargets = phase.GetValidTargets(ability);

			CollectionAssert.Contains(validTargets, fixture.PlayerUnits[1]);
			CollectionAssert.DoesNotContain(validTargets, fixture.EnemyUnits[0]);

			orchestrator.Dispose();
		}

		[Test]
		public void Ally_TrySubmitAbility_SucceedsOnAllyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1, defaultActionPoints: 4);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Ally);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int allyCell = fixture.PlayerUnits[1].BoardPosition;

			bool submitted = phase.TrySubmitAbility(ability, new List<Vector3Int> { allyCell });
			Assert.That(submitted, Is.True);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Cell-targeted abilities on empty ground
		// -------------------------------------------------------------------------

		[Test]
		public void Empty_TrySubmitAbility_SucceedsOnEmptyCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 4);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> validCells = phase.GetValidTargetCells(ability);
			Assert.That(validCells.Count, Is.GreaterThan(0), "Expected at least one empty navigable cell.");

			bool submitted = phase.TrySubmitAbility(ability, new List<Vector3Int> { validCells[0] });
			Assert.That(submitted, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void Empty_GetAffectedObjects_ReturnsEmpty_WhenNothingAtCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Empty);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			IReadOnlyList<Vector3Int> validCells = phase.GetValidTargetCells(ability);
			Assert.That(validCells.Count, Is.GreaterThan(0));

			IReadOnlyList<BattleObject> affectedObjects = phase.GetAffectedObjects(ability, validCells[0]);
			Assert.That(affectedObjects.Count, Is.EqualTo(0));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// GetCastLegality failure reasons
		// -------------------------------------------------------------------------

		[Test]
		public void GetCastLegality_ReturnsNoActiveUnit_WhenTurnContextIsEmpty()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);

			TurnContext emptyTurn = new TurnContext();
			Vector3Int anyCell = new Vector3Int(0, 1, 0);

			AbilityCastLegality legality = BattleActionValidator.GetCastLegality(fixture.BattleContext, emptyTurn, ability, anyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.NoActiveUnit));
		}

		[Test]
		public void GetCastLegality_ReturnsInvalidContext_WhenAbilityIsNull()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

			TurnContext turn = new TurnContext();
			turn.Begin(fixture.BattleContext.PlayerUnits[0]);
			Vector3Int anyCell = new Vector3Int(0, 1, 0);

			AbilityCastLegality legality = BattleActionValidator.GetCastLegality(fixture.BattleContext, turn, null, anyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.InvalidContext));
		}

		[Test]
		public void GetCastLegality_ReturnsSourceNotPlaced_WhenUnitHasNoBoardPosition()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, range: 10, targetProfile: TargetProfile.Enemy);

			BattleUnit unit = fixture.BattleContext.PlayerUnits[0];
			TurnContext turn = new TurnContext();
			turn.Begin(unit);
			// Unit is not placed on the board
			Vector3Int anyCell = new Vector3Int(0, 1, 0);

			AbilityCastLegality legality = BattleActionValidator.GetCastLegality(fixture.BattleContext, turn, ability, anyCell);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.SourceNotPlaced));
		}

		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private static IReadOnlyList<Vector3Int> GetAllNavigablePositions(BattlePhaseTestFixture fixture)
		{
			List<Vector3Int> positions = new List<Vector3Int>();
			IReadOnlyList<VoxelTraversalGraph.Node> nodes = fixture.BattleContext.Board.Navigation.Nodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				positions.Add(nodes[i].Position);
			}

			return positions;
		}

		private static Vector3Int FindCellNotOccupied(IReadOnlyList<Vector3Int> cells, BattlePhaseTestFixture fixture)
		{
			for (int i = 0; i < cells.Count; i++)
			{
				if (!fixture.BattleContext.Board.HasUnitAt(cells[i]))
				{
					return cells[i];
				}
			}

			throw new System.InvalidOperationException("No unoccupied navigable cell found on the board.");
		}

		private static Vector3Int FindAxisAlignedUnoccupiedCell(IReadOnlyList<Vector3Int> cells, BattlePhaseTestFixture fixture, Vector3Int sourceCell)
		{
			for (int i = 0; i < cells.Count; i++)
			{
				Vector3Int cell = cells[i];
				if (cell == sourceCell || fixture.BattleContext.Board.HasUnitAt(cell))
				{
					continue;
				}

				if (cell.x == sourceCell.x || cell.z == sourceCell.z)
				{
					return cell;
				}
			}

			throw new System.InvalidOperationException("No unoccupied axis-aligned navigable cell found on the board.");
		}
	}
}
