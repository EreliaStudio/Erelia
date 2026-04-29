using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Effects
{

public sealed class RemoveInteractiveObjectEffectTests : EffectTestBase
{
	[Test]
	public void Apply_RemovesMatchingObjectAtAffectedCell()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		InteractionObject interactionObject = CreateInteractionObject("trap");
		Vector3Int cell = new Vector3Int(1, 1, 1);

		new PlaceInteractiveObjectEffect
		{
			InteractionObject = interactionObject
		}.Apply(CreateContext(fixture.PlayerUnits[0], null, fixture.BattleContext, p_affectedCell: cell));

		new RemoveInteractiveObjectEffect
		{
			Tags = new List<string> { "trap" }
		}.Apply(CreateContext(p_battleContext: fixture.BattleContext, p_affectedCell: cell));

		Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell).Count, Is.EqualTo(0));
	}

	[Test]
	public void Apply_DoesNotRemoveNonMatchingObject()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		InteractionObject interactionObject = CreateInteractionObject("trap");
		Vector3Int cell = new Vector3Int(1, 1, 1);

		new PlaceInteractiveObjectEffect
		{
			InteractionObject = interactionObject
		}.Apply(CreateContext(fixture.PlayerUnits[0], null, fixture.BattleContext, p_affectedCell: cell));

		new RemoveInteractiveObjectEffect
		{
			Tags = new List<string> { "fire" }
		}.Apply(CreateContext(p_battleContext: fixture.BattleContext, p_affectedCell: cell));

		Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell).Count, Is.EqualTo(1));
	}

	[Test]
	public void Apply_RemovesOnlyObjectsAtAffectedCell()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		InteractionObject interactionObject = CreateInteractionObject("trap");
		Vector3Int firstCell = new Vector3Int(1, 1, 1);
		Vector3Int secondCell = new Vector3Int(2, 1, 1);

		new PlaceInteractiveObjectEffect
		{
			InteractionObject = interactionObject
		}.Apply(CreateContext(fixture.PlayerUnits[0], null, fixture.BattleContext, p_affectedCell: firstCell));

		new PlaceInteractiveObjectEffect
		{
			InteractionObject = interactionObject
		}.Apply(CreateContext(fixture.PlayerUnits[0], null, fixture.BattleContext, p_affectedCell: secondCell));

		new RemoveInteractiveObjectEffect
		{
			Tags = new List<string> { "trap" }
		}.Apply(CreateContext(p_battleContext: fixture.BattleContext, p_affectedCell: firstCell));

		Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(firstCell).Count, Is.EqualTo(0));
		Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(secondCell).Count, Is.EqualTo(1));
	}

	[Test]
	public void Apply_WithMultipleTags_RemovesObjectMatchingAnyTag()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		InteractionObject interactionObject = CreateInteractionObject("trap", "fire");
		Vector3Int cell = new Vector3Int(1, 1, 1);

		new PlaceInteractiveObjectEffect
		{
			InteractionObject = interactionObject
		}.Apply(CreateContext(fixture.PlayerUnits[0], null, fixture.BattleContext, p_affectedCell: cell));

		new RemoveInteractiveObjectEffect
		{
			Tags = new List<string> { "ice", "fire" }
		}.Apply(CreateContext(p_battleContext: fixture.BattleContext, p_affectedCell: cell));

		Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell).Count, Is.EqualTo(0));
	}
}


}