using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Feats
{
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

			// Source gets both DealDamage and MaxSingleHit events
			Assert.That(sourceUnit.PendingFeatEvents.Count, Is.EqualTo(2));
			Assert.That(sourceUnit.PendingFeatEvents[0], Is.InstanceOf<DealDamageRequirement.Event>());
			var dealEvent = (DealDamageRequirement.Event)sourceUnit.PendingFeatEvents[0];
			Assert.That(dealEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void DamageEffect_Apply_RecordsMaxSingleHitDamageEventOnSourceUnit()
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

			Assert.That(sourceUnit.PendingFeatEvents[1], Is.InstanceOf<MaxSingleHitDamageRequirement.Event>());
			var maxHitEvent = (MaxSingleHitDamageRequirement.Event)sourceUnit.PendingFeatEvents[1];
			Assert.That(maxHitEvent.Amount, Is.EqualTo(10));
		}

		[Test]
		public void DamageEffect_Apply_MaxSingleHitEventCarriesClampedAppliedAmount()
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

			// Target had only 5 HP — applied damage is clamped; MaxSingleHit must match
			var maxHitEvent = (MaxSingleHitDamageRequirement.Event)sourceUnit.PendingFeatEvents[1];
			Assert.That(maxHitEvent.Amount, Is.EqualTo(5));
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
			int maxHitAmount = ((MaxSingleHitDamageRequirement.Event)sourceUnit.PendingFeatEvents[1]).Amount;
			int takenAmount = ((TakeDamageRequirement.Event)targetUnit.PendingFeatEvents[0]).Amount;

			// Target had only 5 HP; all three events must reflect the same clamped applied damage
			Assert.That(dealtAmount, Is.EqualTo(5));
			Assert.That(maxHitAmount, Is.EqualTo(5));
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
		// Group 2b — MaxSingleHitDamageRequirement accumulation mode
		// -------------------------------------------------------------------------

		[Test]
		public void MaxSingleHitRequirement_TwoWeakHitsDoNotProgress()
		{
			// Ability scope: each hit evaluated independently — two 30% hits never reach 100%
			var requirement = new MaxSingleHitDamageRequirement { RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new MaxSingleHitDamageRequirement.Event { Amount = 30 } });
			progress.RegisterEvents(new[] { new MaxSingleHitDamageRequirement.Event { Amount = 30 } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void MaxSingleHitRequirement_HitAtThresholdCompletes()
		{
			// A single hit at exactly 100% should complete the requirement
			var requirement = new MaxSingleHitDamageRequirement { RequiredAmount = 50 };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new MaxSingleHitDamageRequirement.Event { Amount = 50 } });

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void MaxSingleHitRequirement_TwoWeakHitsDoNotCompleteNode()
		{
			// Two hits of 25 with RequiredAmount=50 must not complete the node
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new MaxSingleHitDamageRequirement { RequiredAmount = 50 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};

			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
			creatureUnit.Species = ScriptableObject.CreateInstance<CreatureSpecies>();
			creatureUnit.Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(creatureUnit);

			FeatProgressionService.RegisterEvent(creatureUnit, new MaxSingleHitDamageRequirement.Event { Amount = 25 });
			FeatProgressionService.RegisterEvent(creatureUnit, new MaxSingleHitDamageRequirement.Event { Amount = 25 });

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creatureUnit, maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two weak hits must not satisfy a max-single-hit requirement.");

			Object.DestroyImmediate(creatureUnit.Species);
		}

		[Test]
		public void MaxSingleHitRequirement_SingleHitAtThresholdCompletesNode()
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new MaxSingleHitDamageRequirement { RequiredAmount = 50 }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};

			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};
			creatureUnit.Species = ScriptableObject.CreateInstance<CreatureSpecies>();
			creatureUnit.Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(creatureUnit);

			FeatProgressionService.RegisterEvent(creatureUnit, new MaxSingleHitDamageRequirement.Event { Amount = 50 });

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creatureUnit, maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0), "A single hit meeting the threshold must complete the node.");

			Object.DestroyImmediate(creatureUnit.Species);
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

		[Test]
		public void PlayerVictory_MaxSingleHitNodeCompleted_WhenSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new MaxSingleHitDamageRequirement { RequiredAmount = 30 }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(maxHitNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			// A single hit of 30 meets the threshold exactly
			fixture.PlayerUnits[0].RecordFeatEvent(new MaxSingleHitDamageRequirement.Event { Amount = 30 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		[Test]
		public void PlayerVictory_MaxSingleHitNodeNotCompleted_WhenNoSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new MaxSingleHitDamageRequirement { RequiredAmount = 50 }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(maxHitNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			// Two hits of 25 each — neither meets the 50-threshold individually
			fixture.PlayerUnits[0].RecordFeatEvent(new MaxSingleHitDamageRequirement.Event { Amount = 25 });
			fixture.PlayerUnits[0].RecordFeatEvent(new MaxSingleHitDamageRequirement.Event { Amount = 25 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two hits of 25 must not complete a max-single-hit-50 node.");

			orchestrator.Dispose();
		}

		[Test]
		public void DealDamageRequirement_AbilityScope_SingleEventBelowThresholdDoesNotComplete()
		{
			// Ability scope: each event is independent — 40% never reaches 100%
			var requirement = new DealDamageRequirement
			{
				RequiredAmount = 100,
				RequirementScope = FeatRequirement.Scope.Ability
			};
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 40 } });
			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 40 } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void RegisterFightEvents_FightDurationDoesNotCarryPartialProgressAcrossFights()
		{
			CreatureSpecies species = null;
			try
			{
				CreatureUnit creature = CreateCreatureWithRequirement(
					new DealDamageRequirement { RequiredAmount = 100 },
					out species,
					out FeatNode damageNode);

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 50 } });

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 50 } });

				FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creature, damageNode);
				Assert.That(nodeProgress, Is.Not.Null);
				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(0));
			}
			finally
			{
				if (species != null)
				{
					Object.DestroyImmediate(species);
				}
			}
		}

		[Test]
		public void RegisterFightEvents_FightDurationCanCompleteWithinOneFight()
		{
			CreatureSpecies species = null;
			try
			{
				CreatureUnit creature = CreateCreatureWithRequirement(
					new DealDamageRequirement { RequiredAmount = 100 },
					out species,
					out FeatNode damageNode);

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase>
					{
						new DealDamageRequirement.Event { Amount = 50 },
						new DealDamageRequirement.Event { Amount = 50 }
					});

				FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creature, damageNode);
				Assert.That(nodeProgress, Is.Not.Null);
				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(1));
			}
			finally
			{
				if (species != null)
				{
					Object.DestroyImmediate(species);
				}
			}
		}

		[Test]
		public void RegisterFightEvents_GameDurationCarriesPartialProgressAcrossFights()
		{
			CreatureSpecies species = null;
			try
			{
				CreatureUnit creature = CreateCreatureWithRequirement(
					new DealDamageRequirement
					{
						RequiredAmount = 100,
						RequirementScope = FeatRequirement.Scope.Game
					},
					out species,
					out FeatNode damageNode);

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 50 } });

				FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creature, damageNode);
				Assert.That(nodeProgress, Is.Not.Null);
				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(0));
				Assert.That(nodeProgress.RequirementProgress[0].CurrentProgress, Is.EqualTo(50f).Within(0.01f));

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 50 } });

				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(1));
			}
			finally
			{
				if (species != null)
				{
					Object.DestroyImmediate(species);
				}
			}
		}

		[Test]
		public void RegisterFightEvents_RepeatableFightDurationCompletesOncePerQualifyingFight()
		{
			CreatureSpecies species = null;
			try
			{
				CreatureUnit creature = CreateCreatureWithRequirement(
					new DealDamageRequirement
					{
						RequiredAmount = 100,
						RequiredRepeatCount = 2,
						RequirementScope = FeatRequirement.Scope.Fight
					},
					out species,
					out FeatNode damageNode);

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 100 } });
				FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creature, damageNode);
				Assert.That(nodeProgress, Is.Not.Null);
				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(0));

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase> { new DealDamageRequirement.Event { Amount = 100 } });

				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(1));
			}
			finally
			{
				if (species != null)
				{
					Object.DestroyImmediate(species);
				}
			}
		}

		[Test]
		public void RegisterFightEvents_RepeatableAbilityScopeCompletesForEachQualifyingEvent()
		{
			CreatureSpecies species = null;
			try
			{
				CreatureUnit creature = CreateCreatureWithRequirement(
					new DealDamageRequirement
					{
						RequiredAmount = 50,
						RequirementScope = FeatRequirement.Scope.Ability,
						RequiredRepeatCount = 2
					},
					out species,
					out FeatNode damageNode);

				FeatProgressionService.RegisterFightEvents(
					creature,
					new List<FeatRequirement.EventBase>
					{
						new DealDamageRequirement.Event { Amount = 50 },
						new DealDamageRequirement.Event { Amount = 50 }
					});

				FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creature, damageNode);
				Assert.That(nodeProgress, Is.Not.Null);
				Assert.That(nodeProgress.CompletionCount, Is.EqualTo(1));
			}
			finally
			{
				if (species != null)
				{
					Object.DestroyImmediate(species);
				}
			}
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

		private static CreatureUnit CreateCreatureWithRequirement(
			FeatRequirement requirement,
			out CreatureSpecies species,
			out FeatNode requirementNode,
			int numberOfRepeatTime = 0)
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			requirementNode = new FeatNode
			{
				Id = "requirement_node",
				DisplayName = "Requirement",
				Requirements = new List<FeatRequirement> { requirement },
				NeighbourNodeIds = new List<string> { rootNode.Id },
				NumberOfRepeatTime = numberOfRepeatTime
			};
			rootNode.NeighbourNodeIds.Add(requirementNode.Id);

			species = ScriptableObject.CreateInstance<CreatureSpecies>();
			species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, requirementNode },
				RootNodeId = rootNode.Id
			};

			var creature = new CreatureUnit
			{
				Species = species,
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			FeatProgressionService.InitializeCreatureUnit(creature);
			return creature;
		}
	}
}
