using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Phases.Placement
{
	public sealed class PlacementPhaseTests
	{
		[Test]
		public void Enter_PlacesEnemiesInsideEnemyZone()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 2);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);

			IReadOnlyList<Vector3Int> enemyCells = placementPhase.GetEnemyPlacementCells();
			for (int index = 0; index < fixture.EnemyUnits.Length; index++)
			{
				Assert.That(fixture.EnemyUnits[index].HasBoardPosition, Is.True);
				Assert.That(enemyCells, Contains.Item(fixture.EnemyUnits[index].BoardPosition));
			}
		}

		[Test]
		public void GetPlacementMaskCells_ReturnsPlayerCellsForOverlay()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);

			IReadOnlyList<Vector3Int> playerCells = placementPhase.GetPlacementMaskCells();

			Assert.That(playerCells.Count, Is.GreaterThan(0));
			CollectionAssert.AreEquivalent(placementPhase.GetPlayerPlacementCells(), playerCells);
		}

		[Test]
		public void BattleMaskRules_ApplyMask_CanRenderPlacementCellsInOverlay()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);
			IReadOnlyList<Vector3Int> playerCells = placementPhase.GetPlacementMaskCells();
			BattleMaskRules.ApplyMask(fixture.OverlayState, playerCells, VoxelMask.Placement);

			for (int index = 0; index < playerCells.Count; index++)
			{
				Assert.That(fixture.HasPlacementMask(playerCells[index]), Is.True);
			}
		}

		[Test]
		public void GetPlayerPlacementCells_ReturnsOnlyPlayerSideCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			IReadOnlyList<Vector3Int> playerCells = placementPhase.GetPlayerPlacementCells();
			Assert.That(playerCells.Count, Is.GreaterThan(0));
			for (int index = 0; index < playerCells.Count; index++)
			{
				Assert.That(playerCells[index].z, Is.LessThan(fixture.BattleContext.Board.Terrain.SizeZ / 2));
			}
		}

		[Test]
		public void GetEnemyPlacementCells_ReturnsOnlyEnemySideCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			IReadOnlyList<Vector3Int> enemyCells = placementPhase.GetEnemyPlacementCells();
			Assert.That(enemyCells.Count, Is.GreaterThan(0));
			for (int index = 0; index < enemyCells.Count; index++)
			{
				Assert.That(enemyCells[index].z, Is.GreaterThanOrEqualTo(fixture.BattleContext.Board.Terrain.SizeZ / 2));
			}
		}

		[Test]
		public void GetUnitsWaitingForPlacement_ReturnsAllUnplacedPlayerUnits()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			Assert.That(placementPhase.GetUnitsWaitingForPlacement().Count, Is.EqualTo(2));
		}

		[Test]
		public void GetValidPlacementCells_WithBattleUnit_ReturnsNonEmptyLegalCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			IReadOnlyList<Vector3Int> validCells = placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0]);
			Assert.That(validCells.Count, Is.GreaterThan(0));
			for (int index = 0; index < validCells.Count; index++)
			{
				Assert.That(placementPhase.GetPlayerPlacementCells(), Contains.Item(validCells[index]));
			}
		}

		[Test]
		public void GetValidPlacementCells_WithCreatureUnit_ReturnsSameCellsAsBattleUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			CollectionAssert.AreEquivalent(
				placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0]),
				placementPhase.GetValidPlacementCells(fixture.PlayerSources[0]));
		}

		[Test]
		public void CanPlaceUnit_WithBattleUnit_RejectsEnemyUnits()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			Assert.That(placementPhase.CanPlaceUnit(fixture.PlayerUnits[0]), Is.True);
			Assert.That(placementPhase.CanPlaceUnit(fixture.EnemyUnits[0]), Is.False);
		}

		[Test]
		public void CanPlaceUnit_WithCreatureUnitAndCell_RejectsOutsidePlayerZone()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());
			Vector3Int enemyCell = placementPhase.GetEnemyPlacementCells()[0];

			Assert.That(placementPhase.CanPlaceUnit(fixture.PlayerSources[0], enemyCell), Is.False);
		}

		[Test]
		public void CanPlaceUnit_WithBattleUnitAndCell_RejectsOccupiedCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);

			Vector3Int firstCell = placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0])[0];
			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], firstCell), Is.True);

			Assert.That(placementPhase.CanPlaceUnit(fixture.PlayerUnits[1], firstCell), Is.False);
		}

		[Test]
		public void TryPlaceUnit_WithBattleUnit_PlacesAndMovesThatUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());
			IReadOnlyList<Vector3Int> validCells = placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0]);

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], validCells[0]), Is.True);
			Assert.That(fixture.PlayerUnits[0].BoardPosition, Is.EqualTo(validCells[0]));

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], validCells[1]), Is.True);
			Assert.That(fixture.PlayerUnits[0].BoardPosition, Is.EqualTo(validCells[1]));
		}

		[Test]
		public void TryPlaceUnit_WithCreatureUnit_PlacesThatUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());
			Vector3Int cell = placementPhase.GetValidPlacementCells(fixture.PlayerSources[0])[0];

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerSources[0], cell), Is.True);
			Assert.That(fixture.PlayerUnits[0].BoardPosition, Is.EqualTo(cell));
		}

		[Test]
		public void GetPlacementMaskCells_WithSelectedUnit_ReturnsOnlyThatUnitsValidCells()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0])[0]), Is.True);
			Assert.That(placementPhase.TrySelectPlayerUnit(fixture.PlayerUnits[1]), Is.True);

			CollectionAssert.AreEquivalent(
				placementPhase.GetValidPlacementCells(fixture.PlayerUnits[1]),
				placementPhase.GetPlacementMaskCells());
		}

		[Test]
		public void TryRemovePlayerUnit_RemovesPlacedUnitAndReselectsIt()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());
			Vector3Int cell = placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0])[0];

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], cell), Is.True);
			Assert.That(placementPhase.TryRemovePlayerUnit(fixture.PlayerUnits[0]), Is.True);

			Assert.That(fixture.PlayerUnits[0].HasBoardPosition, Is.False);
			Assert.That(placementPhase.GetSelectedPlayerUnit(), Is.SameAs(fixture.PlayerUnits[0]));
		}

		[Test]
		public void TryRemovePlayerUnitAt_RemovesPlayerUnitOccupyingCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());
			Vector3Int cell = placementPhase.GetValidPlacementCells(fixture.PlayerUnits[0])[0];

			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerUnits[0], cell), Is.True);
			Assert.That(placementPhase.TryRemovePlayerUnitAt(cell), Is.True);

			Assert.That(fixture.PlayerUnits[0].HasBoardPosition, Is.False);
		}

		[Test]
		public void CanCompletePlacement_ReturnsFalseUntilAllPlayerUnitsArePlaced()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			PlacementPhase placementPhase = fixture.GetPlacementPhase(fixture.CreateInitializedOrchestrator());

			Assert.That(placementPhase.CanCompletePlacement(), Is.False);
			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerSources[0], placementPhase.GetValidPlacementCells(fixture.PlayerSources[0])[0]), Is.True);
			Assert.That(placementPhase.CanCompletePlacement(), Is.False);
			Assert.That(placementPhase.TryPlaceUnit(fixture.PlayerSources[1], placementPhase.GetValidPlacementCells(fixture.PlayerSources[1])[0]), Is.True);
			Assert.That(placementPhase.CanCompletePlacement(), Is.True);
		}

		[Test]
		public void TryCompletePlacement_StartsCombatWithoutDependingOnOverlayMasks()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			PlacementPhase placementPhase = fixture.GetPlacementPhase(orchestrator);

			fixture.PlaceAllPlayers(placementPhase);
			fixture.SetTurnBars(
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max, 0f },
				enemyTurnBars: new[] { 0f });

			Assert.That(placementPhase.TryCompletePlacement(), Is.True);
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
		}
	}
}
