using NUnit.Framework;

namespace Tests.Battle.Phases.Initiative
{
	public sealed class StaminaRatioTests
	{
		private const float Delta = 0.0001f;

		// -------------------------------------------------------------------------
		// Different Stamina values → different advancement rates
		// -------------------------------------------------------------------------

		[Test]
		public void AdvanceTurnBars_HigherStamina_AdvancesFaster()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1,
				playerRecoveries: new[] { 4f },
				enemyRecoveries: new[] { 4f });

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 2f }, enemyStaminaRatios: new[] { 1f });

			BattleTurnRules.AdvanceTurnBars(fixture.BattleContext, 1f);

			BattleUnit fastUnit = fixture.PlayerUnits[0];
			BattleUnit slowUnit = fixture.EnemyUnits[0];

			Assert.That(fastUnit.BattleAttributes.TurnBar.Current, Is.EqualTo(2f).Within(Delta));
			Assert.That(slowUnit.BattleAttributes.TurnBar.Current, Is.EqualTo(1f).Within(Delta));
			Assert.That(fastUnit.BattleAttributes.TurnBar.Current, Is.GreaterThan(slowUnit.BattleAttributes.TurnBar.Current));
		}

		[Test]
		public void AdvanceTurnBars_LowerStamina_AdvancesSlower()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1,
				playerRecoveries: new[] { 4f },
				enemyRecoveries: new[] { 4f });

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 0.5f }, enemyStaminaRatios: new[] { 1f });

			BattleTurnRules.AdvanceTurnBars(fixture.BattleContext, 2f);

			BattleUnit slowUnit = fixture.PlayerUnits[0];
			BattleUnit normalUnit = fixture.EnemyUnits[0];

			Assert.That(slowUnit.BattleAttributes.TurnBar.Current, Is.EqualTo(1f).Within(Delta));
			Assert.That(normalUnit.BattleAttributes.TurnBar.Current, Is.EqualTo(2f).Within(Delta));
		}

		[Test]
		public void TryFindNextActiveUnit_HigherStamina_ReachesReadyFirst()
		{
			// Both units start at 0. Player has Stamina 2, enemy Stamina 1, same Recovery.
			// Player bar fills at twice the rate, so player becomes ready first.
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1,
				playerRecoveries: new[] { 4f },
				enemyRecoveries: new[] { 4f });

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 2f }, enemyStaminaRatios: new[] { 1f });

			bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

			Assert.That(found, Is.True);
			Assert.That(selected, Is.SameAs(fixture.PlayerUnits[0]));
		}

		// -------------------------------------------------------------------------
		// Stunned unit (StaminaRatio = 0) does not advance
		// -------------------------------------------------------------------------

		[Test]
		public void AdvanceTurnBars_StaminaRatioZero_DoesNotAdvance()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1);

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 0f }, enemyStaminaRatios: new[] { 1f });

			BattleTurnRules.AdvanceTurnBars(fixture.BattleContext, 2f);

			BattleUnit stunnedUnit = fixture.PlayerUnits[0];
			BattleUnit normalUnit = fixture.EnemyUnits[0];

			Assert.That(stunnedUnit.BattleAttributes.TurnBar.Current, Is.EqualTo(0f).Within(Delta));
			Assert.That(normalUnit.BattleAttributes.TurnBar.Current, Is.GreaterThan(0f));
		}

		[Test]
		public void TryFindNextActiveUnit_AllUnitsStunned_ReturnsNoUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1);

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 0f }, enemyStaminaRatios: new[] { 0f });

			bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

			Assert.That(found, Is.False);
			Assert.That(selected, Is.Null);
		}

		[Test]
		public void TryFindNextActiveUnit_StunnedUnit_OtherUnitActsInstead()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1,
				playerRecoveries: new[] { 4f },
				enemyRecoveries: new[] { 4f });

			fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
			fixture.SetStaminaRatios(playerStaminaRatios: new[] { 0f }, enemyStaminaRatios: new[] { 1f });

			bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

			Assert.That(found, Is.True);
			Assert.That(selected, Is.SameAs(fixture.EnemyUnits[0]));
		}
	}
}
