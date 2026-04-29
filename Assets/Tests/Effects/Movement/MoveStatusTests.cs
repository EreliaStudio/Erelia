using NUnit.Framework;
using UnityEngine;
using Tests.Effects;

namespace Tests.Effects.Movement
{
	public sealed class MoveStatusTests : EffectTestBase
	{
		[Test]
		public void Apply_MovesTargetAwayFromCaster()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(1, 0, 1));

			new MoveStatus
			{
				ForceOrientation = MoveStatus.Orientation.AwayFromCaster,
				Force = 1
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(2, 0, 1)));
		}

		[Test]
		public void Apply_WithForceTwo_MovesTargetTwoCellsAwayFromCaster()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(1, 0, 1));

			new MoveStatus
			{
				ForceOrientation = MoveStatus.Orientation.AwayFromCaster,
				Force = 2
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(3, 0, 1)));
		}

		[Test]
		public void Apply_WithZeroForce_DoesNotMoveTarget()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];
			Vector3Int targetCell = new Vector3Int(1, 0, 1);

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, targetCell);

			new MoveStatus
			{
				ForceOrientation = MoveStatus.Orientation.AwayFromCaster,
				Force = 0
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(targetCell));
		}
	}
}
