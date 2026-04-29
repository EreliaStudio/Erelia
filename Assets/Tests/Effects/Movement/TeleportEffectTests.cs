using NUnit.Framework;
using UnityEngine;
using Tests.Effects;

namespace Tests.Effects.Movement
{
	public sealed class TeleportEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_MovesTargetRelativeToCaster()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(2, 0, 1));

			new TeleportEffect
			{
				Destination = new Vector3Int(1, 0, 0),
				RelativeToCaster = true
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(1, 0, 1)));
		}

		[Test]
		public void Apply_WithAbsoluteDestination_MovesTargetToDestination()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			BattleUnit target = fixture.EnemyUnits[0];

			PlaceUnit(fixture.BattleContext, source, new Vector3Int(0, 0, 1));
			PlaceUnit(fixture.BattleContext, target, new Vector3Int(1, 0, 1));

			new TeleportEffect
			{
				Destination = new Vector3Int(2, 0, 1),
				RelativeToCaster = false
			}.Apply(CreateContext(source, target, fixture.BattleContext));

			Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(2, 0, 1)));
		}
	}
}
