using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.LineOfSight
{
	public sealed class LineOfSightTests
	{
		// -------------------------------------------------------------------------
		// Same cell
		// -------------------------------------------------------------------------

		[Test]
		public void SameCell_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(3, 3, 3, 3), Is.True);
		}

		// -------------------------------------------------------------------------
		// Adjacent cells — always clear
		// -------------------------------------------------------------------------

		[Test]
		public void Adjacent_AlongZAxis_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(5, 5, 5, 6), Is.True);
		}

		[Test]
		public void Adjacent_AlongXAxis_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(5, 5, 6, 5), Is.True);
		}

		[Test]
		public void Adjacent_Diagonal_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(5, 5, 6, 6), Is.True);
		}

		// -------------------------------------------------------------------------
		// Long clear lines
		// -------------------------------------------------------------------------

		[Test]
		public void LongClear_AlongZAxis_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(5, 0, 5, 10), Is.True);
		}

		[Test]
		public void LongClear_AlongXAxis_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(0, 5, 10, 5), Is.True);
		}

		[Test]
		public void LongClear_Diagonal_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(0, 0, 8, 8), Is.True);
		}

		[Test]
		public void LongClear_ArbitraryAngle_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			// Non-axis, non-diagonal angle
			Assert.That(f.HasLoS(1, 1, 7, 4), Is.True);
		}

		// -------------------------------------------------------------------------
		// Single wall blocking Z-axis shot
		// -------------------------------------------------------------------------

		[Test]
		public void SingleWall_BlocksZAxisShot_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			// Source at z=2, wall at z=5, target at z=8 — all at x=5
			Assert.That(f.HasLoS(5, 2, 5, 8), Is.False);
		}

		[Test]
		public void SingleWall_BlocksXAxisShot_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			// Source at x=2, wall at x=5, target at x=8 — all at z=5
			Assert.That(f.HasLoS(2, 5, 8, 5), Is.False);
		}

		[Test]
		public void SingleWall_BlocksDiagonalShot_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			// Diagonal from (2,2) to (8,8), wall at (5,5) is on the path
			Assert.That(f.HasLoS(2, 2, 8, 8), Is.False);
		}

		// -------------------------------------------------------------------------
		// Wall that does NOT block the shot
		// -------------------------------------------------------------------------

		[Test]
		public void WallBesidePath_DoesNotBlockShot_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(6, 6)   // next to the shot path, not on it
				.Build();

			// Shot at z=5, wall offset at z=6
			Assert.That(f.HasLoS(5, 2, 5, 10), Is.True);
		}

		[Test]
		public void WallAdjacentToSource_DoesNotBlockShot_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(4, 5)
				.Build();

			// Source at (5,5), wall at (4,5) — beside but not in front
			Assert.That(f.HasLoS(5, 5, 5, 9), Is.True);
		}

		[Test]
		public void WallAdjacentToTarget_DoesNotBlockShot_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(6, 9)
				.Build();

			// Target at (5,9), wall at (6,9) — beside but not on the path
			Assert.That(f.HasLoS(5, 5, 5, 9), Is.True);
		}

		// -------------------------------------------------------------------------
		// Wall at source or target is ignored
		// -------------------------------------------------------------------------

		[Test]
		public void WallAtSourceCell_IsIgnored_ReturnsTrue()
		{
			// Source and target cells are excluded from the blocking check per the LoS rules
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(2, 2)   // wall placed exactly at source
				.Build();

			Assert.That(f.HasLoS(2, 2, 8, 2), Is.True);
		}

		[Test]
		public void WallAtTargetCell_IsIgnored_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(8, 2)   // wall placed exactly at target
				.Build();

			Assert.That(f.HasLoS(2, 2, 8, 2), Is.True);
		}

		// -------------------------------------------------------------------------
		// Multiple walls
		// -------------------------------------------------------------------------

		[Test]
		public void MultipleWalls_AllOnPath_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 5)
				.WithWall(6, 5)
				.Build();

			// Two walls along the Z=5 axis, shot from x=0 to x=10
			Assert.That(f.HasLoS(0, 5, 10, 5), Is.False);
		}

		[Test]
		public void MultipleWalls_OnlyOneOnPath_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)   // on path
				.WithWall(5, 2)   // off the direct shot
				.Build();

			Assert.That(f.HasLoS(5, 0, 5, 10), Is.False);
		}

		[Test]
		public void MultipleWalls_NoneOnPath_ReturnsTrue()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 4)
				.WithWall(7, 6)
				.Build();

			// Shot along x=5, z=0 to z=10 — neither wall is on that line
			Assert.That(f.HasLoS(5, 0, 5, 10), Is.True);
		}

		// -------------------------------------------------------------------------
		// Wall corridor: gap in a wall allows LoS through it
		// -------------------------------------------------------------------------

		[Test]
		public void WallWithGap_LoSThroughGap_ReturnsTrue()
		{
			// Solid wall across z=5 except at x=5, which is the gap
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 5)
				.WithWall(4, 5)
				// x=5 intentionally left open
				.WithWall(6, 5)
				.WithWall(7, 5)
				.Build();

			// Shot through the gap at x=5
			Assert.That(f.HasLoS(5, 2, 5, 8), Is.True);
		}

		[Test]
		public void WallWithGap_LoSBlockedElsewhere_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 5)
				.WithWall(4, 5)
				.WithWall(6, 5)
				.WithWall(7, 5)
				.Build();

			// Shot at x=4 hits the wall (no gap there)
			Assert.That(f.HasLoS(4, 2, 4, 8), Is.False);
		}

		// -------------------------------------------------------------------------
		// Out-of-board cells
		// -------------------------------------------------------------------------

		[Test]
		public void SourceOutsideBoard_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Vector3Int outside = new Vector3Int(-1, 1, 0);
			Vector3Int inside = new Vector3Int(5, 1, 5);

			Assert.That(f.HasLoS(outside, inside), Is.False);
		}

		[Test]
		public void TargetOutsideBoard_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Vector3Int inside = new Vector3Int(5, 1, 5);
			Vector3Int outside = new Vector3Int(99, 1, 99);

			Assert.That(f.HasLoS(inside, outside), Is.False);
		}

		[Test]
		public void BothOutsideBoard_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Vector3Int a = new Vector3Int(-5, 1, -5);
			Vector3Int b = new Vector3Int(99, 1, 99);

			Assert.That(f.HasLoS(a, b), Is.False);
		}

		// -------------------------------------------------------------------------
		// Symmetry: LoS A→B equals LoS B→A
		// -------------------------------------------------------------------------

		[Test]
		public void LoS_IsSymmetric_WhenClear()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(2, 2, 8, 6), Is.EqualTo(f.HasLoS(8, 6, 2, 2)));
		}

		[Test]
		public void LoS_IsSymmetric_WhenBlocked()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 4)
				.Build();

			bool forward = f.HasLoS(2, 4, 9, 4);
			bool reverse = f.HasLoS(9, 4, 2, 4);
			Assert.That(forward, Is.EqualTo(reverse));
			Assert.That(forward, Is.False);
		}

		// -------------------------------------------------------------------------
		// L-shaped obstacle: clear shot around it
		// -------------------------------------------------------------------------

		[Test]
		public void LShapedObstacle_ShotAroundIt_ReturnsTrue()
		{
			//  . . . . . .
			//  . . W . . .
			//  . . W W . .
			//  S . . . . T
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(2, 7)
				.WithWall(2, 8)
				.WithWall(3, 8)
				.Build();

			// Horizontal shot at z=6 — the L-shape is at z=7 and z=8
			Assert.That(f.HasLoS(0, 6, 8, 6), Is.True);
		}

		[Test]
		public void LShapedObstacle_ShotThroughIt_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(2, 7)
				.WithWall(2, 8)
				.WithWall(3, 8)
				.Build();

			// Shot at x=2 from z=5 to z=10 — hits the wall at (2,7)
			Assert.That(f.HasLoS(2, 5, 2, 10), Is.False);
		}

		// -------------------------------------------------------------------------
		// Ability validation: RequireLineOfSight = true
		// -------------------------------------------------------------------------

		[Test]
		public void Ability_RequiresLoS_ReturnsFalse_WhenWallBlocks()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.BlockedByLineOfSight));
		}

		[Test]
		public void Ability_RequiresLoS_ReturnsValid_WhenPathClear()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.True);
		}

		[Test]
		public void Ability_NoLoSRequired_ReturnsValid_EvenWhenWallBlocks()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: false);

			Assert.That(legality.IsValid, Is.True);
		}

		[Test]
		public void Ability_RequiresLoS_WallBesidePath_ReturnsValid()
		{
			// Wall is beside the shot, not on it
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(6, 5)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.True);
		}

		[Test]
		public void Ability_RequiresLoS_MultipleWallsBlock_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 4)
				.WithWall(5, 6)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.BlockedByLineOfSight));
		}

		[Test]
		public void Ability_RequiresLoS_DiagonalClearShot_ReturnsValid()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();

			f.PlaceUnit(f.PlayerUnit, 2, 2);
			f.PlaceUnit(f.EnemyUnit, 8, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.True);
		}

		[Test]
		public void Ability_RequiresLoS_DiagonalBlockedShot_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 2, 2);
			f.PlaceUnit(f.EnemyUnit, 8, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);

			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason, Is.EqualTo(AbilityCastLegality.Failure.BlockedByLineOfSight));
		}

		// -------------------------------------------------------------------------
		// Walkable voxels at standing level are transparent to LoS
		// -------------------------------------------------------------------------

		[Test]
		public void WalkableVoxel_OnPath_DoesNotBlockLoS()
		{
			// Default fixture fills y=1 with Walkable — shot through clear walkable space
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			Assert.That(f.HasLoS(5, 0, 5, 10), Is.True,
				"Walkable voxels must not block line-of-sight");
		}

		[Test]
		public void ObstacleWall_OnPath_BlocksLoS_WhileWalkableDoesNot()
		{
			// Two paths on the same board: one through clear walkable space, one through a wall
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)   // Obstacle at (5,5)
				.Build();

			// Clear path (x=4, no wall)
			Assert.That(f.HasLoS(4, 0, 4, 10), Is.True,
				"Walkable path beside the wall must remain open");

			// Blocked path (x=5, wall at z=5)
			Assert.That(f.HasLoS(5, 0, 5, 10), Is.False,
				"Obstacle wall must block line-of-sight");
		}

		[Test]
		public void WalkableVoxels_FullBoard_LongDiagonalClear()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();
			// All intermediate cells are Walkable — diagonal should always be clear
			Assert.That(f.HasLoS(0, 0, 11, 11), Is.True);
		}

		[Test]
		public void WalkableVoxels_ReplacedByObstacle_BlocksOnlyThatCell()
		{
			// Replace a single walkable cell with an obstacle and verify only that shot is blocked
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 3)
				.Build();

			Assert.That(f.HasLoS(0, 0, 6, 6), Is.False,
				"Wall at (3,3) is on the diagonal from (0,0) to (6,6)");
			Assert.That(f.HasLoS(0, 0, 6, 9), Is.True,
				"Different angle avoids the wall at (3,3)");
		}

		// -------------------------------------------------------------------------
		// Unit placement via injected nav nodes (ability-level tests)
		// -------------------------------------------------------------------------

		[Test]
		public void PlaceUnit_Succeeds_OnWalkableCell()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();

			bool placed = f.PlaceUnit(f.PlayerUnit, 3, 3);
			Assert.That(placed, Is.True, "Unit placement must succeed on a walkable cell");
		}

		[Test]
		public void PlaceUnit_Fails_WhenStandingSpaceIsBlockedByObstacle()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(3, 3)
				.Build();

			bool placed = f.PlaceUnitAt(f.PlayerUnit, new Vector3Int(3, LineOfSightTestFixture.FloorY, 3));
			Assert.That(placed, Is.False, "Unit placement must fail when the space above the floor is blocked");
		}

		[Test]
		public void Ability_RequiresLoS_WalkablePath_BothUnitsPlaced_ReturnsValid()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create().Build();

			bool p = f.PlaceUnit(f.PlayerUnit, 5, 2);
			bool e = f.PlaceUnit(f.EnemyUnit, 5, 8);
			Assert.That(p && e, Is.True, "Both units must be placed successfully");

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);
			Assert.That(legality.IsValid, Is.True,
				"Clear walkable path between units must allow LoS ability");
		}

		[Test]
		public void Ability_RequiresLoS_ObstacleWallBlocks_BothUnitsPlaced_ReturnsFalse()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			bool p = f.PlaceUnit(f.PlayerUnit, 5, 2);
			bool e = f.PlaceUnit(f.EnemyUnit, 5, 8);
			Assert.That(p && e, Is.True, "Both units must be placed successfully");

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);
			Assert.That(legality.IsValid, Is.False);
			Assert.That(legality.FailureReason,
				Is.EqualTo(AbilityCastLegality.Failure.BlockedByLineOfSight));
		}

		[Test]
		public void Ability_NoLoSRequired_WalkableOrObstacle_AlwaysValid()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(5, 5)
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: false);
			Assert.That(legality.IsValid, Is.True,
				"Ability not requiring LoS must ignore obstacles");
		}

		[Test]
		public void Ability_RequiresLoS_WallBesideWalkablePath_ReturnsValid()
		{
			using LineOfSightTestFixture f = LineOfSightTestFixture.Create()
				.WithWall(6, 5)   // beside the shot at x=5, not on it
				.Build();

			f.PlaceUnit(f.PlayerUnit, 5, 2);
			f.PlaceUnit(f.EnemyUnit, 5, 8);

			AbilityCastLegality legality = f.GetAbilityCastLegality(requireLoS: true);
			Assert.That(legality.IsValid, Is.True,
				"Wall beside the path must not block the ability");
		}
	}
}
