using NUnit.Framework;
using UnityEngine;
using Tests.Effects;

namespace Tests.Effects.Movement
{
	public sealed class TeleportEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_MovesTargetToDestination()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(2, 0, 1));

			new TeleportEffect
			{
				Destination = new Vector3Int(4, 0, 3)
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(4, 0, 3)));
		}

		[Test]
		public void Apply_DoesNotMoveCaster()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(1, 0, 1));

			new TeleportEffect
			{
				Destination = new Vector3Int(2, 0, 1)
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(source.BoardPosition, Is.EqualTo(new Vector3Int(0, 0, 1)));
		}
	}
}
