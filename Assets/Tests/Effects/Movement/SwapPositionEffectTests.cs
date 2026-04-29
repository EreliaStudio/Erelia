using NUnit.Framework;
using UnityEngine;

namespace Tests.Effects
{

public sealed class SwapPositionEffectTests : EffectTestBase
{
	[Test]
	public void Apply_SwapsSourceAndTargetCells()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		BattleUnit source = fixture.PlayerUnits[0];
		BattleUnit target = fixture.EnemyUnits[0];

		Vector3Int sourceCell = new Vector3Int(1, 1, 1);
		Vector3Int targetCell = new Vector3Int(2, 1, 1);

		PlaceUnit(fixture.BattleContext, source, sourceCell);
		PlaceUnit(fixture.BattleContext, target, targetCell);

		new SwapPositionEffect()
			.Apply(CreateContext(source, target, fixture.BattleContext));

		Assert.That(source.BoardPosition, Is.EqualTo(targetCell));
		Assert.That(target.BoardPosition, Is.EqualTo(sourceCell));
	}

	[Test]
	public void Apply_CanSwapBackAfterFirstSwap()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		BattleUnit source = fixture.PlayerUnits[0];
		BattleUnit target = fixture.EnemyUnits[0];

		Vector3Int sourceCell = new Vector3Int(1, 1, 1);
		Vector3Int targetCell = new Vector3Int(2, 1, 1);

		PlaceUnit(fixture.BattleContext, source, sourceCell);
		PlaceUnit(fixture.BattleContext, target, targetCell);

		new SwapPositionEffect()
			.Apply(CreateContext(source, target, fixture.BattleContext));

		new SwapPositionEffect()
			.Apply(CreateContext(source, target, fixture.BattleContext));

		Assert.That(source.BoardPosition, Is.EqualTo(sourceCell));
		Assert.That(target.BoardPosition, Is.EqualTo(targetCell));
	}
}


}