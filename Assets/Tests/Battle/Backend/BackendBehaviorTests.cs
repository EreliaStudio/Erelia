using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Backend
{
	public sealed class BackendBehaviorTests
	{
		// -------------------------------------------------------------------------
		// Failed resolution does not advance turn flow
		// -------------------------------------------------------------------------

		[Test]
		public void Resolution_RevertsToPreviousPhase_WhenResolverReturnsFalse()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 2);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase playerTurnPhase = fixture.GetPlayerTurnPhase(orchestrator);
			BattleUnit player = fixture.PlayerUnits[0];

			// Drain all MP so the move will fail inside the resolver
			fixture.SetResources(player, actionPoints: 2, movementPoints: 0);
			Vector3Int destination = new Vector3Int(player.BoardPosition.x + 1, player.BoardPosition.y, player.BoardPosition.z);

			// Bypass the phase API and inject a pending action the resolver will reject
			Assert.That(orchestrator.TurnContext.TrySetPendingAction(new MoveAction(player, destination)), Is.True);
			orchestrator.TransitionTo(BattlePhaseType.Resolution);

			// Should revert to PlayerTurn, not advance
			Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.PlayerTurn));
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(player));

			orchestrator.Dispose();
		}

		[Test]
		public void Resolution_DoesNotEndTurn_WhenResolverReturnsFalse()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 2);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			fixture.SetResources(player, actionPoints: 2, movementPoints: 0);
			Vector3Int unreachableDestination = new Vector3Int(player.BoardPosition.x + 1, player.BoardPosition.y, player.BoardPosition.z);

			Assert.That(orchestrator.TurnContext.TrySetPendingAction(new MoveAction(player, unreachableDestination)), Is.True);
			orchestrator.TransitionTo(BattlePhaseType.Resolution);

			// Active unit must still be the same player — turn was not consumed
			Assert.That(orchestrator.TurnContext.ActiveUnit, Is.SameAs(player));
			Assert.That(player.BoardPosition, Is.Not.EqualTo(unreachableDestination));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Defeat flow: board removal + UnitDefeated event
		// -------------------------------------------------------------------------

		[Test]
		public void DefeatUnit_RemovesUnitFromBoard()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1);
			Ability killingAbility = fixture.CreateDamageAbility(baseDamage: 999, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(killingAbility);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			BattleUnit enemy = fixture.EnemyUnits[0];

			Assert.That(enemy.HasBoardPosition, Is.True);
			phase.TrySubmitAbility(killingAbility, new[] { enemy.BoardPosition });

			Assert.That(enemy.IsDefeated, Is.True);
			Assert.That(enemy.HasBoardPosition, Is.False);
			Assert.That(fixture.BattleContext.Board.HasUnitAt(enemy.BoardPosition), Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void DefeatUnit_FiresUnitDefeatedEvent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultActionPoints: 1);
			Ability killingAbility = fixture.CreateDamageAbility(baseDamage: 999, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(killingAbility);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			BattleUnit enemy = fixture.EnemyUnits[0];

			BattleUnit capturedDefeated = null;
			fixture.BattleContext.UnitDefeated += u => capturedDefeated = u;

			phase.TrySubmitAbility(killingAbility, new[] { enemy.BoardPosition });

			Assert.That(capturedDefeated, Is.SameAs(enemy));

			orchestrator.Dispose();
		}

		[Test]
		public void DefeatUnit_DefeatedUnitNoLongerBlocksMovement()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 2, defaultActionPoints: 2, defaultMovement: 10);
			Ability killingAbility = fixture.CreateDamageAbility(baseDamage: 999, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(killingAbility);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f, 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			BattleUnit enemy = fixture.EnemyUnits[0];
			Vector3Int enemyCell = enemy.BoardPosition;

			phase.TrySubmitAbility(killingAbility, new[] { enemyCell });

			// The formerly occupied cell should now be reachable
			IReadOnlyList<Vector3Int> reachable = phase.GetReachableCells();
			CollectionAssert.Contains(reachable, enemyCell);

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Status hooks: TurnStart / TurnEnd
		// -------------------------------------------------------------------------

		[Test]
		public void TurnStartHook_FiresOnTurnBegin_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20);
			Status burnStatus = CreateDamageStatus(fixture, StatusHookPoint.TurnStart, damage: 3);

			BattleUnit player = fixture.BattleContext.PlayerUnits[0];
			player.Statuses.Add(burnStatus, 1, new Duration { Type = Duration.Kind.Infinite });

			BattleTurnRules.BeginTurn(fixture.BattleContext, player);

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(20));
		}

		[Test]
		public void TurnEndHook_FiresOnTurnEnd_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20);
			Status burnStatus = CreateDamageStatus(fixture, StatusHookPoint.TurnEnd, damage: 2);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(burnStatus, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			fixture.GetPlayerTurnPhase(orchestrator).TrySubmitEndTurn();

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Status hooks: BeforeMove / AfterMove
		// -------------------------------------------------------------------------

		[Test]
		public void BeforeMoveHook_FiresBeforeMove_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultMovement: 2);
			Status status = CreateDamageStatus(fixture, StatusHookPoint.BeforeMove, damage: 3);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = phase.GetReachableCells()[0];
			phase.TrySubmitMove(destination);

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		[Test]
		public void AfterMoveHook_FiresAfterMove_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultMovement: 2);
			Status status = CreateDamageStatus(fixture, StatusHookPoint.AfterMove, damage: 3);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = phase.GetReachableCells()[0];
			phase.TrySubmitMove(destination);

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		[Test]
		public void BeforeMoveHook_UnitPositionNotYetChanged_WhenHookFires()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultMovement: 2);

			// The hook fires before the move, so the unit is still at its starting cell
			Vector3Int capturedPosition = default;
			Status status = CreateCallbackStatus(fixture, StatusHookPoint.BeforeMove, ctx =>
			{
				if (ctx.TargetObject is BattleUnit u)
				{
					capturedPosition = u.BoardPosition;
				}
			});

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			Vector3Int startCell = player.BoardPosition;
			player.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = phase.GetReachableCells()[0];
			phase.TrySubmitMove(destination);

			Assert.That(capturedPosition, Is.EqualTo(startCell));

			orchestrator.Dispose();
		}

		[Test]
		public void AfterMoveHook_UnitPositionAlreadyChanged_WhenHookFires()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultMovement: 2);

			Vector3Int capturedPosition = default;
			Status status = CreateCallbackStatus(fixture, StatusHookPoint.AfterMove, ctx =>
			{
				if (ctx.TargetObject is BattleUnit u)
				{
					capturedPosition = u.BoardPosition;
				}
			});

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = phase.GetReachableCells()[0];
			phase.TrySubmitMove(destination);

			Assert.That(capturedPosition, Is.EqualTo(destination));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Status hooks: BeforeCastingAnAbility / AfterCastingAnAbility
		// -------------------------------------------------------------------------

		[Test]
		public void BeforeCastingAnAbilityHook_FiresBeforeCast_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultActionPoints: 4);
			Ability abilityToUse = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(abilityToUse);

			Status hookStatus = CreateDamageStatus(fixture, StatusHookPoint.BeforeCastingAnAbility, damage: 3);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(hookStatus, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitAbility(abilityToUse, new[] { fixture.EnemyUnits[0].BoardPosition });

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		[Test]
		public void AfterCastingAnAbilityHook_FiresAfterCast_AndDamagesUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 20, defaultActionPoints: 4);
			Ability abilityToUse = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(abilityToUse);

			Status hookStatus = CreateDamageStatus(fixture, StatusHookPoint.AfterCastingAnAbility, damage: 3);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(hookStatus, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitAbility(abilityToUse, new[] { fixture.EnemyUnits[0].BoardPosition });

			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Status hooks: OnHPLoss / OnAPLoss / OnMPLoss
		// -------------------------------------------------------------------------

		[Test]
		public void OnHPLossHook_FiresWhenHPDecreases_ViaAbility()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 50, defaultActionPoints: 4);
			Ability abilityToUse = fixture.CreateDamageAbility(baseDamage: 5, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(abilityToUse);

			Status onHpLoss = CreateDamageStatus(fixture, StatusHookPoint.OnHPLoss, damage: 2);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit enemy = fixture.EnemyUnits[0];
			enemy.Statuses.Add(onHpLoss, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = enemy.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitAbility(abilityToUse, new[] { enemy.BoardPosition });

			// Primary damage (5) + OnHPLoss reaction damage (2) = 7 total
			Assert.That(enemy.BattleAttributes.Health.Current, Is.LessThanOrEqualTo(healthBefore - 7));

			orchestrator.Dispose();
		}

		[Test]
		public void OnAPLossHook_FiresWhenAPDecreases_ViaAbilityUse()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 50, defaultActionPoints: 4);
			Ability abilityToUse = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1, targetProfile: TargetProfile.Enemy);
			fixture.PlayerSources[0].Abilities.Add(abilityToUse);

			Status onApLoss = CreateDamageStatus(fixture, StatusHookPoint.OnAPLoss, damage: 3);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(onApLoss, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitAbility(abilityToUse, new[] { fixture.EnemyUnits[0].BoardPosition });

			// AP was spent → OnAPLoss fired → caster took 3 damage
			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		[Test]
		public void OnMPLossHook_FiresWhenMPDecreases_ViaMove()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultHealth: 50, defaultMovement: 2);

			Status onMpLoss = CreateDamageStatus(fixture, StatusHookPoint.OnMPLoss, damage: 4);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			player.Statuses.Add(onMpLoss, 1, new Duration { Type = Duration.Kind.Infinite });
			int healthBefore = player.BattleAttributes.Health.Current;

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int destination = phase.GetReachableCells()[0];
			phase.TrySubmitMove(destination);

			// MP was spent → OnMPLoss fired → unit took 4 damage
			Assert.That(player.BattleAttributes.Health.Current, Is.LessThan(healthBefore));

			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Turn-based status duration decay
		// -------------------------------------------------------------------------

		[Test]
		public void TurnBasedStatus_ExpiresAfterDuration()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 0, defaultActionPoints: 0);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			Status tempStatus = ScriptableObject.CreateInstance<Status>();
			tempStatus.HookPoint = StatusHookPoint.TurnStart;
			tempStatus.Effects = new System.Collections.Generic.List<Effect>();

			player.Statuses.Add(tempStatus, 1, new Duration { Type = Duration.Kind.TurnBased, Turns = 1 });
			Assert.That(player.Statuses.Contains(tempStatus), Is.True);

			// End the turn: AdvanceTurnDurations should remove the 1-turn status
			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitEndTurn();

			Assert.That(player.Statuses.Contains(tempStatus), Is.False);

			UnityEngine.Object.DestroyImmediate(tempStatus);
			orchestrator.Dispose();
		}

		[Test]
		public void InfiniteStatus_DoesNotExpireOnTurnEnd()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1, defaultMovement: 0, defaultActionPoints: 0);
			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			BattleUnit player = fixture.PlayerUnits[0];
			Status infiniteStatus = ScriptableObject.CreateInstance<Status>();
			infiniteStatus.HookPoint = StatusHookPoint.TurnStart;
			infiniteStatus.Effects = new System.Collections.Generic.List<Effect>();

			player.Statuses.Add(infiniteStatus, 1, new Duration { Type = Duration.Kind.Infinite });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			phase.TrySubmitEndTurn();

			Assert.That(player.Statuses.Contains(infiniteStatus), Is.True);

			UnityEngine.Object.DestroyImmediate(infiniteStatus);
			orchestrator.Dispose();
		}

		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		private Status CreateDamageStatus(BattlePhaseTestFixture fixture, StatusHookPoint hookPoint, int damage)
		{
			Status status = ScriptableObject.CreateInstance<Status>();
			status.HookPoint = hookPoint;
			status.Effects = new List<Effect>
			{
				new DamageTargetEffect
				{
					Input = new MathFormula.DamageInput
					{
						BaseDamage = damage,
						DamageKind = MathFormula.DamageInput.Kind.Physical,
						AttackRatio = 0f,
						MagicRatio = 0f
					}
				}
			};

			// Track for cleanup via reflection-free approach — destroy inline in Dispose is fine
			// since tests are short-lived; register by passing to a local list
			_ownedStatuses.Add(status);
			return status;
		}

		private Status CreateCallbackStatus(BattlePhaseTestFixture fixture, StatusHookPoint hookPoint, System.Action<BattleAbilityExecutionContext> callback)
		{
			Status status = ScriptableObject.CreateInstance<Status>();
			status.HookPoint = hookPoint;
			status.Effects = new List<Effect> { new CallbackEffect(callback) };
			_ownedStatuses.Add(status);
			return status;
		}

		private readonly List<UnityEngine.Object> _ownedStatuses = new List<UnityEngine.Object>();

		[TearDown]
		public void TearDown()
		{
			for (int i = 0; i < _ownedStatuses.Count; i++)
			{
				if (_ownedStatuses[i] != null)
				{
					UnityEngine.Object.DestroyImmediate(_ownedStatuses[i]);
				}
			}

			_ownedStatuses.Clear();
		}

		// Minimal Effect subclass used only in tests to capture the execution context
		private sealed class CallbackEffect : Effect
		{
			private readonly System.Action<BattleAbilityExecutionContext> _callback;

			public CallbackEffect(System.Action<BattleAbilityExecutionContext> callback)
			{
				_callback = callback;
			}

			public override void Apply(BattleAbilityExecutionContext context)
			{
				_callback?.Invoke(context);
			}
		}
	}
}
