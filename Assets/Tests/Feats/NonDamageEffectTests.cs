using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.NonDamageEffect
{
	public sealed class NonDamageEffectTests
	{
		[Test]
		public void Apply_RecordsTurnBarTimeAdjustedEvent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 100);
			using BattleFeatEventCapture capture = new BattleFeatEventCapture();
			BattleUnit sourceUnit = fixture.PlayerUnits[0];
			BattleUnit targetUnit = fixture.EnemyUnits[0];

			BattleAbilityExecutionContext context = new BattleAbilityExecutionContext
			{
				BattleContext = fixture.BattleContext,
				SourceObject = sourceUnit,
				TargetObject = targetUnit
			};

			var effect = new AdjustTurnBarTimeEffect { Delta = 1f };
			effect.Apply(context);

			Assert.That(capture.Find<TurnBarTimeAdjustedEvent>(sourceUnit), Is.Not.Null);
			Assert.That(capture.Find<TurnBarTimeAdjustedEvent>(targetUnit), Is.Not.Null);
		}
	}
}
