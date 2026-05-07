using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Feats.CastDamageAbility
{
	public sealed class CastDamageAbilityTests
	{
		[Test]
		public void CasterAccumulatesDealDamageEvent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			Ability damageAbility = fixture.CreateDamageAbility(baseDamage: 5, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(damageAbility);

			using BattleFeatEventCapture capture = new BattleFeatEventCapture();

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurn = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			playerTurn.TrySubmitAbility(damageAbility, new List<Vector3Int> { enemyCell });

			BattleUnit caster = fixture.PlayerUnits[0];
			Assert.That(capture.Find<DealDamageRequirement.Event>(caster), Is.Not.Null);

			orchestrator.Dispose();
		}

		[Test]
		public void TargetAccumulatesTakeDamageEvent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			Ability damageAbility = fixture.CreateDamageAbility(baseDamage: 5, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(damageAbility);

			using BattleFeatEventCapture capture = new BattleFeatEventCapture();

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurn = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			playerTurn.TrySubmitAbility(damageAbility, new List<Vector3Int> { enemyCell });

			BattleUnit target = fixture.EnemyUnits[0];
			Assert.That(capture.Find<TakeDamageRequirement.Event>(target), Is.Not.Null);

			orchestrator.Dispose();
		}
	}
}
