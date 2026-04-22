using NUnit.Framework;
using UnityEngine;

public sealed class InitiativeTurnOrderTests
{
	// -------------------------------------------------------------------------
	// Player-vs-enemy tie: player goes first
	// -------------------------------------------------------------------------

	[Test]
	public void Tie_PlayerAndEnemy_BothReady_PlayerGoesFirst()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		fixture.SetTurnBars(
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { fixture.EnemyUnits[0].BattleAttributes.TurnBar.Max });

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.PlayerUnits[0]));
	}

	// -------------------------------------------------------------------------
	// Player-vs-player tie: lower team index goes first
	// -------------------------------------------------------------------------

	[Test]
	public void Tie_TwoPlayersReady_FirstInTeamOrderGoesFirst()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);

		fixture.SetTurnBars(
			playerTurnBars: new[]
			{
				fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max,
				fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max
			},
			enemyTurnBars: new[] { 0f });

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.PlayerUnits[0]));
	}

	[Test]
	public void Tie_TwoPlayersReady_SecondPlayerGoesFirst_IfFirstIsSkipped()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 1);

		// Only second player is ready
		fixture.SetTurnBars(
			playerTurnBars: new[] { 0f, fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.PlayerUnits[1]));
	}

	// -------------------------------------------------------------------------
	// Enemy-vs-enemy tie: lower team index goes first
	// -------------------------------------------------------------------------

	[Test]
	public void Tie_TwoEnemiesReady_FirstInTeamOrderGoesFirst()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 2);

		fixture.SetTurnBars(
			playerTurnBars: new[] { 0f },
			enemyTurnBars: new[]
			{
				fixture.EnemyUnits[0].BattleAttributes.TurnBar.Max,
				fixture.EnemyUnits[1].BattleAttributes.TurnBar.Max
			});

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.EnemyUnits[0]));
	}

	[Test]
	public void Tie_TwoEnemiesReady_SecondEnemyGoesFirst_IfFirstIsSkipped()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 2);

		fixture.SetTurnBars(
			playerTurnBars: new[] { 0f },
			enemyTurnBars: new[] { 0f, fixture.EnemyUnits[1].BattleAttributes.TurnBar.Max });

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.EnemyUnits[1]));
	}

	// -------------------------------------------------------------------------
	// Turn bar advancement selects the fastest unit
	// -------------------------------------------------------------------------

	[Test]
	public void TurnBarAdvancement_SelectsFastestUnit_WhenNoneReady()
	{
		// Two units with different recoveries — the faster one should be selected
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1, enemyCount: 1,
			playerRecoveries: new[] { 8f },
			enemyRecoveries: new[] { 4f });

		fixture.SetTurnBars(
			playerTurnBars: new[] { 0f },
			enemyTurnBars: new[] { 0f });

		bool found = BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out BattleUnit selected);

		Assert.That(found, Is.True);
		Assert.That(selected, Is.SameAs(fixture.EnemyUnits[0]));
	}

	[Test]
	public void TurnBarAdvancement_AllUnitsAdvance_WhenNoneReady()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);

		float playerBarBefore = fixture.PlayerUnits[0].BattleAttributes.TurnBar.Current;
		float enemyBarBefore = fixture.EnemyUnits[0].BattleAttributes.TurnBar.Current;

		fixture.SetTurnBars(playerTurnBars: new[] { 0f }, enemyTurnBars: new[] { 0f });
		BattleTurnRules.TryFindNextActiveUnit(fixture.BattleContext, out _);

		// After advancement, at least one unit must have a full bar
		float playerBarAfter = fixture.PlayerUnits[0].BattleAttributes.TurnBar.Current;
		float enemyBarAfter = fixture.EnemyUnits[0].BattleAttributes.TurnBar.Current;

		Assert.That(playerBarAfter > playerBarBefore || enemyBarAfter > enemyBarBefore, Is.True);
	}

	// -------------------------------------------------------------------------
	// Unit becomes ready during another unit's turn
	// -------------------------------------------------------------------------

	[Test]
	public void UnitBecomesReady_DuringAnotherUnitsTurn_ActsAfterCurrentTurnEnds()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 2, enemyCount: 1,
			defaultMovement: 0, defaultActionPoints: 0);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();

		// Player[0] goes first; Player[1] is almost ready (will be ready after turn bar reset)
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[]
			{
				fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max,
				fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max  // also ready
			},
			enemyTurnBars: new[] { 0f });

		Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[0]));

		// Player[0] ends their turn → Player[1] should be next (already ready)
		PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
		phase.TrySubmitEndTurn();

		Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[1]));

		orchestrator.Dispose();
	}

	// -------------------------------------------------------------------------
	// No-action turn continuation vs forced turn end
	// -------------------------------------------------------------------------

	[Test]
	public void CanContinueTurn_ReturnsFalse_WhenNoMovementAndNoAbilities()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1, enemyCount: 1,
			defaultMovement: 0, defaultActionPoints: 0);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		bool canContinue = BattleTurnRules.CanContinueTurn(fixture.BattleContext, orchestrator.TurnContext);

		Assert.That(canContinue, Is.False);

		orchestrator.Dispose();
	}

	[Test]
	public void CanContinueTurn_ReturnsTrue_WhenUnitCanStillMove()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1, enemyCount: 1,
			defaultMovement: 2, defaultActionPoints: 0);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		bool canContinue = BattleTurnRules.CanContinueTurn(fixture.BattleContext, orchestrator.TurnContext);

		Assert.That(canContinue, Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CanContinueTurn_ReturnsTrue_WhenUnitHasUsableAbility()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1, enemyCount: 1,
			defaultMovement: 0, defaultActionPoints: 2);

		Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, range: 10, targetProfile: TargetProfile.Enemy);
		fixture.PlayerSources[0].Abilities.Add(ability);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		bool canContinue = BattleTurnRules.CanContinueTurn(fixture.BattleContext, orchestrator.TurnContext);

		Assert.That(canContinue, Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CanContinueTurn_ReturnsFalse_AfterSpendingAllResources()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1, enemyCount: 1,
			defaultMovement: 1, defaultActionPoints: 1);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		fixture.SetResources(orchestrator.TurnContext.ActiveUnit, actionPoints: 0, movementPoints: 0);

		bool canContinue = BattleTurnRules.CanContinueTurn(fixture.BattleContext, orchestrator.TurnContext);
		Assert.That(canContinue, Is.False);

		orchestrator.Dispose();
	}

	[Test]
	public void TurnEnds_Automatically_WhenNoActionsRemain_WithZeroResources()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 2, enemyCount: 1,
			defaultMovement: 0, defaultActionPoints: 0);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(orchestrator,
			playerTurnBars: new[]
			{
				fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max,
				fixture.PlayerUnits[1].BattleAttributes.TurnBar.Max
			},
			enemyTurnBars: new[] { 0f });

		// Player[0] is active; they have no AP, no MP, no abilities → turn should auto-end via EndTurn
		PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
		Assert.That(phase.TrySubmitEndTurn(), Is.True);

		// After Player[0] ends turn, Player[1] should be active
		Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(fixture.PlayerUnits[1]));

		orchestrator.Dispose();
	}
}
