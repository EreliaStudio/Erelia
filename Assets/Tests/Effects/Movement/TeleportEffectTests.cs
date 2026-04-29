using NUnit.Framework;
using UnityEngine;

namespace Tests.Effects
{

public sealed class TeleportEffectTests : EffectTestBase
{
	[Test]
	public void Apply_MovesTargetRelativeToCaster()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		BattleUnit source = fixture.PlayerUnits[0];
		BattleUnit target = fixture.EnemyUnits[0];

		PlaceUnit(fixture.BattleContext, source, new Vector3Int(1, 1, 1));
		PlaceUnit(fixture.BattleContext, target, new Vector3Int(3, 1, 1));

		new TeleportEffect
		{
			Destination = new Vector3Int(1, 0, 0),
			RelativeToCaster = true
		}.Apply(CreateContext(source, target, fixture.BattleContext));

		Assert.That(target.BoardPosition, Is.EqualTo(new Vector3Int(2, 1, 1)));
	}

	[Test]
	public void Apply_WithZeroRelativeOffset_MovesTargetToCasterCellIfAllowedByEffectRules()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		BattleUnit source = fixture.PlayerUnits[0];
		BattleUnit target = fixture.EnemyUnits[0];
		Vector3Int targetCell = new Vector3Int(3, 1, 1);

		PlaceUnit(fixture.BattleContext, source, new Vector3Int(1, 1, 1));
		PlaceUnit(fixture.BattleContext, target, targetCell);

		new TeleportEffect
		{
			Destination = Vector3Int.zero,
			RelativeToCaster = true
		}.Apply(CreateContext(source, target, fixture.BattleContext));

		Assert.That(target.BoardPosition, Is.Not.EqualTo(targetCell));
	}
}

}