using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Tests.Effects;

namespace Tests.Effects.BoardObjects
{
	public sealed class PlaceInteractiveObjectEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_AddsObjectAtAffectedCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			InteractionObject interactionObject = CreateInteractionObject("trap");
			Vector3Int cell = new Vector3Int(1, 0, 1);

			new PlaceInteractiveObjectEffect
			{
				InteractionObject = interactionObject,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 2 }
			}.Apply(CreateContext(source, null, fixture.BattleContext, p_affectedCell: cell));

			IReadOnlyList<BattleInteractiveObject> objects =
				fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell);

			Assert.That(objects.Count, Is.EqualTo(1));
			Assert.That(objects[0].InteractionObject, Is.SameAs(interactionObject));
			Assert.That(objects[0].Side, Is.EqualTo(BattleSide.Player));
			Assert.That(objects[0].RemainingDuration.Turns, Is.EqualTo(2));
			CollectionAssert.Contains(objects[0].Tags, "trap");
		}

		[Test]
		public void Apply_CopiesAllInteractionObjectTags()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			InteractionObject interactionObject = CreateInteractionObject("trap", "fire", "hazard");
			Vector3Int cell = new Vector3Int(1, 0, 1);

			new PlaceInteractiveObjectEffect
			{
				InteractionObject = interactionObject,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 2 }
			}.Apply(CreateContext(source, null, fixture.BattleContext, p_affectedCell: cell));

			IReadOnlyList<BattleInteractiveObject> objects =
				fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell);

			Assert.That(objects.Count, Is.EqualTo(1));
			CollectionAssert.Contains(objects[0].Tags, "trap");
			CollectionAssert.Contains(objects[0].Tags, "fire");
			CollectionAssert.Contains(objects[0].Tags, "hazard");
		}

		[Test]
		public void Apply_UsesEnemySideWhenSourceIsEnemy()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.EnemyUnits[0];
			InteractionObject interactionObject = CreateInteractionObject("trap");
			Vector3Int cell = new Vector3Int(1, 0, 1);

			new PlaceInteractiveObjectEffect
			{
				InteractionObject = interactionObject,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 2 }
			}.Apply(CreateContext(source, null, fixture.BattleContext, p_affectedCell: cell));

			IReadOnlyList<BattleInteractiveObject> objects =
				fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell);

			Assert.That(objects.Count, Is.EqualTo(1));
			Assert.That(objects[0].Side, Is.EqualTo(BattleSide.Enemy));
		}

		[Test]
		public void Apply_CanPlaceMultipleObjectsAtSameCell()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit source = fixture.PlayerUnits[0];
			InteractionObject firstObject = CreateInteractionObject("trap");
			InteractionObject secondObject = CreateInteractionObject("fire");
			Vector3Int cell = new Vector3Int(1, 0, 1);

			new PlaceInteractiveObjectEffect
			{
				InteractionObject = firstObject
			}.Apply(CreateContext(source, null, fixture.BattleContext, p_affectedCell: cell));

			new PlaceInteractiveObjectEffect
			{
				InteractionObject = secondObject
			}.Apply(CreateContext(source, null, fixture.BattleContext, p_affectedCell: cell));

			Assert.That(fixture.BattleContext.Board.Runtime.GetInteractiveObjects(cell).Count, Is.EqualTo(2));
		}
	}
}
