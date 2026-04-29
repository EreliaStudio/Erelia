using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class FeatProgressionTests
{
	// -------------------------------------------------------------------------
	// Group 1 — Effect-level: verify events are written into context units
	// -------------------------------------------------------------------------

	[Test]
	public void DamageEffect_Apply_RecordsDealDamageEventOnSourceUnit()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new DamageTargetEffect
		{
			Input = new MathFormula.DamageInput
			{
				BaseDamage = 10,
				DamageKind = MathFormula.DamageInput.Kind.Physical,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		};

		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.EqualTo(1));
		Assert.That(sourceUnit.PendingFeatEvents[0], Is.InstanceOf<DealDamageRequirement.Event>());
		var dealEvent = (DealDamageRequirement.Event)sourceUnit.PendingFeatEvents[0];
		Assert.That(dealEvent.Amount, Is.EqualTo(10));
	}

	[Test]
	public void DamageEffect_Apply_RecordsTakeDamageEventOnTargetUnit()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new DamageTargetEffect
		{
			Input = new MathFormula.DamageInput
			{
				BaseDamage = 10,
				DamageKind = MathFormula.DamageInput.Kind.Physical,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		};

		effect.Apply(context);

		Assert.That(targetUnit.PendingFeatEvents.Count, Is.EqualTo(1));
		Assert.That(targetUnit.PendingFeatEvents[0], Is.InstanceOf<TakeDamageRequirement.Event>());
		var takeEvent = (TakeDamageRequirement.Event)targetUnit.PendingFeatEvents[0];
		Assert.That(takeEvent.Amount, Is.EqualTo(10));
	}

	[Test]
	public void DamageEffect_Apply_BothEventsCarryTheSameAppliedAmount()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 5);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new DamageTargetEffect
		{
			Input = new MathFormula.DamageInput
			{
				BaseDamage = 20,
				DamageKind = MathFormula.DamageInput.Kind.Physical,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		};

		effect.Apply(context);

		int dealtAmount = ((DealDamageRequirement.Event)sourceUnit.PendingFeatEvents[0]).Amount;
		int takenAmount = ((TakeDamageRequirement.Event)targetUnit.PendingFeatEvents[0]).Amount;

		// Target had only 5 HP; the clamped applied amount must match on both sides
		Assert.That(dealtAmount, Is.EqualTo(5));
		Assert.That(takenAmount, Is.EqualTo(5));
	}

	[Test]
	public void DamageEffect_Apply_RecordsNoEventsWhenBaseDamageIsZero()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new DamageTargetEffect
		{
			Input = new MathFormula.DamageInput { BaseDamage = 0 }
		};

		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
		Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
	}

	[Test]
	public void DamageEffect_Apply_RecordsNoEventsWhenTargetAlreadyAtZeroHealth()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 0);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new DamageTargetEffect
		{
			Input = new MathFormula.DamageInput
			{
				BaseDamage = 10,
				DamageKind = MathFormula.DamageInput.Kind.Physical,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		};

		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
		Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
	}

	[Test]
	public void HealEffect_Apply_RecordsHealHealthEventOnSourceUnit()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new HealTargetEffect
		{
			Input = new MathFormula.HealingInput
			{
				BaseHealing = 15,
				AttackRatio = 0f,
				MagicRatio = 0f
			}
		};

		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.EqualTo(1));
		Assert.That(sourceUnit.PendingFeatEvents[0], Is.InstanceOf<HealHealthRequirement.Event>());
		var healEvent = (HealHealthRequirement.Event)sourceUnit.PendingFeatEvents[0];
		Assert.That(healEvent.Amount, Is.EqualTo(15));
	}

	[Test]
	public void HealEffect_Apply_RecordsNoEventOnTargetUnit()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new HealTargetEffect
		{
			Input = new MathFormula.HealingInput { BaseHealing = 15 }
		};

		effect.Apply(context);

		Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
	}

	[Test]
	public void HealEffect_Apply_RecordsNoEventsWhenBaseHealingIsZero()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 50);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new HealTargetEffect
		{
			Input = new MathFormula.HealingInput { BaseHealing = 0 }
		};

		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
	}

	[Test]
	public void NonDamageEffect_Apply_RecordsNoEvents()
	{
		BattleUnit sourceUnit = CreateUnit(BattleSide.Player, health: 100);
		BattleUnit targetUnit = CreateUnit(BattleSide.Enemy, health: 100);
		BattleAbilityExecutionContext context = CreateContext(sourceUnit, targetUnit);

		var effect = new AdjustTurnBarTimeEffect { Delta = 1f };
		effect.Apply(context);

		Assert.That(sourceUnit.PendingFeatEvents.Count, Is.Zero);
		Assert.That(targetUnit.PendingFeatEvents.Count, Is.Zero);
	}

	// -------------------------------------------------------------------------
	// Group 2 — Ability cast via battle phases accumulates events on both units
	// -------------------------------------------------------------------------

	[Test]
	public void CastDamageAbility_CasterAccumulatesDealDamageEvent()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1,
			enemyCount: 1,
			defaultHealth: 50,
			defaultActionPoints: 4);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		Ability damageAbility = fixture.CreateDamageAbility(baseDamage: 5, actionPointCost: 1);
		PlayerTurnPhase playerTurn = fixture.GetPlayerTurnPhase(orchestrator);
		IReadOnlyList<Vector3Int> validCells = playerTurn.GetValidTargetCells(damageAbility);

		Assert.That(validCells.Count, Is.GreaterThan(0), "No valid target cells for damage ability.");

		playerTurn.TrySubmitAbility(damageAbility, new List<Vector3Int> { validCells[0] });
		orchestrator.TransitionTo(BattlePhaseType.Resolution);

		BattleUnit caster = fixture.PlayerUnits[0];
		Assert.That(caster.PendingFeatEvents.Count, Is.GreaterThan(0));

		bool hasDealDamageEvent = false;
		for (int i = 0; i < caster.PendingFeatEvents.Count; i++)
		{
			if (caster.PendingFeatEvents[i] is DealDamageRequirement.Event)
			{
				hasDealDamageEvent = true;
				break;
			}
		}

		Assert.That(hasDealDamageEvent, Is.True);

		orchestrator.Dispose();
	}

	[Test]
	public void CastDamageAbility_TargetAccumulatesTakeDamageEvent()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1,
			enemyCount: 1,
			defaultHealth: 50,
			defaultActionPoints: 4);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		Ability damageAbility = fixture.CreateDamageAbility(baseDamage: 5, actionPointCost: 1);
		PlayerTurnPhase playerTurn = fixture.GetPlayerTurnPhase(orchestrator);
		IReadOnlyList<Vector3Int> validCells = playerTurn.GetValidTargetCells(damageAbility);

		Assert.That(validCells.Count, Is.GreaterThan(0), "No valid target cells for damage ability.");

		playerTurn.TrySubmitAbility(damageAbility, new List<Vector3Int> { validCells[0] });
		orchestrator.TransitionTo(BattlePhaseType.Resolution);

		BattleUnit target = fixture.EnemyUnits[0];
		Assert.That(target.PendingFeatEvents.Count, Is.GreaterThan(0));

		bool hasTakeDamageEvent = false;
		for (int i = 0; i < target.PendingFeatEvents.Count; i++)
		{
			if (target.PendingFeatEvents[i] is TakeDamageRequirement.Event)
			{
				hasTakeDamageEvent = true;
				break;
			}
		}

		Assert.That(hasTakeDamageEvent, Is.True);

		orchestrator.Dispose();
	}

	// -------------------------------------------------------------------------
	// Group 3 — EndPhase applies feat progression on player victory only
	// -------------------------------------------------------------------------

	[Test]
	public void PlayerVictory_FeatEventsAppliedToPlayerCreatureUnit()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1,
			enemyCount: 1,
			defaultHealth: 50,
			defaultActionPoints: 4);

		// Build a feat board with a root node and one deal-damage node
		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode damageNode = new FeatNode
		{
			Id = "deal_damage_node",
			DisplayName = "Damage Dealer",
			Requirements = new List<FeatRequirement>
			{
				new DealDamageRequirement { RequiredAmount = 50 }
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward
				{
					Attribute = BonusStatsReward.AttributeType.Health,
					Value = 10
				}
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(damageNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, damageNode },
			RootNodeId = rootNode.Id
		};

		// Initialize feat board progress (unlocks root node)
		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		// Defeat all enemy units so the outcome is a player victory
		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		// Manually push a DealDamage event that fully satisfies the requirement (50/50 = 100%)
		fixture.PlayerUnits[0].RecordFeatEvent(new DealDamageRequirement.Event { Amount = 50 });

		// Transition to end — EndPhase will apply progression
		orchestrator.TransitionTo(BattlePhaseType.End);

		// The damage node must now be completed
		FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], damageNode);
		Assert.That(nodeProgress, Is.Not.Null);
		Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

		orchestrator.Dispose();
	}

	[Test]
	public void EnemyVictory_FeatEventsNotAppliedToPlayerCreatureUnit()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1,
			enemyCount: 1,
			defaultHealth: 50,
			defaultActionPoints: 4);

		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode damageNode = new FeatNode
		{
			Id = "deal_damage_node",
			DisplayName = "Damage Dealer",
			Requirements = new List<FeatRequirement>
			{
				new DealDamageRequirement { RequiredAmount = 50 }
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward
				{
					Attribute = BonusStatsReward.AttributeType.Health,
					Value = 10
				}
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(damageNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, damageNode },
			RootNodeId = rootNode.Id
		};

		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		// Defeat all player units so the outcome is an enemy victory
		fixture.PlayerUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.PlayerUnits[0]);

		// Push a fully satisfying event — should NOT be applied on enemy victory
		fixture.PlayerUnits[0].RecordFeatEvent(new DealDamageRequirement.Event { Amount = 50 });

		orchestrator.TransitionTo(BattlePhaseType.End);

		FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], damageNode);
		bool wasCompleted = nodeProgress != null && nodeProgress.CompletionCount > 0;
		Assert.That(wasCompleted, Is.False);

		orchestrator.Dispose();
	}

	[Test]
	public void PlayerVictory_FeatEventsNotAppliedToEnemyCreatureUnit()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
			playerCount: 1,
			enemyCount: 1,
			defaultHealth: 50,
			defaultActionPoints: 4);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		// Defeat all enemy units for a player victory
		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		int eventCountBeforeEnd = fixture.EnemyUnits[0].PendingFeatEvents.Count;

		orchestrator.TransitionTo(BattlePhaseType.End);

		// Enemy units must not have had any feat progression applied.
		// Since EnemySources[0] has no species FeatBoard, the best we can verify
		// is that the enemy unit's pending events were not touched by the end phase.
		Assert.That(fixture.EnemyUnits[0].PendingFeatEvents.Count, Is.EqualTo(eventCountBeforeEnd));

		orchestrator.Dispose();
	}

	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	private static BattleUnit CreateUnit(BattleSide side, int health = 100)
	{
		var creatureUnit = new CreatureUnit
		{
			Attributes = new Attributes { Health = health },
			Abilities = new List<Ability>(),
			PermanentPassives = new List<Status>()
		};
		return new BattleUnit(creatureUnit, side);
	}

	private static BattleAbilityExecutionContext CreateContext(BattleUnit source, BattleUnit target)
	{
		return new BattleAbilityExecutionContext
		{
			SourceObject = source,
			TargetObject = target
		};
	}
}
