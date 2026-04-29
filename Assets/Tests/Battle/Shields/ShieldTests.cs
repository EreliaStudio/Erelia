using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Battle.Shields
{
	public sealed class ShieldTests
	{
		[Test]
		public void AddShield_IgnoresNonPositiveAmount()
		{
			BattleUnit unit = CreateUnit();

			unit.BattleAttributes.AddShield(ShieldKind.Physical, 0, durationTurns: 1);
			unit.BattleAttributes.AddShield(ShieldKind.Magical, -5, durationTurns: 1);

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		}

		[Test]
		public void AddShield_RaisesActiveShieldsChanged()
		{
			BattleAttributes attributes = CreateUnit().BattleAttributes;
			int changedCount = 0;
			attributes.ActiveShields.Changed += _ => changedCount++;

			attributes.AddShield(ShieldKind.Physical, 10, durationTurns: 1);

			Assert.That(changedCount, Is.EqualTo(1));
			Assert.That(attributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(attributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Physical));
		}

		[Test]
		public void AbsorbDamage_MatchingShieldAbsorbsBeforeHealth()
		{
			BattleUnit unit = CreateUnit(health: 100);
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

			BattleShieldAbsorptionResult result =
				unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Physical, 15);

			Assert.That(result.AmountAbsorbed, Is.EqualTo(15));
			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(5));
			Assert.That(unit.BattleAttributes.Health.Current, Is.EqualTo(100));
		}

		[Test]
		public void AbsorbDamage_SurvivingShieldRaisesShieldEntryChanged()
		{
			BattleAttributes attributes = CreateUnit().BattleAttributes;
			attributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

			int changedCount = 0;
			int observedAmount = 0;
			attributes.ActiveShields[0].Changed += shield =>
			{
				changedCount++;
				observedAmount = shield.CurrentAmount;
			};

			attributes.AbsorbDamage(MathFormula.DamageInput.Kind.Physical, 15);

			Assert.That(changedCount, Is.EqualTo(1));
			Assert.That(observedAmount, Is.EqualTo(5));
		}

		[Test]
		public void AbsorbDamage_WrongShieldKindDoesNotAbsorb()
		{
			BattleUnit unit = CreateUnit(health: 100);
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

			BattleShieldAbsorptionResult result =
				unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Magical, 15);

			Assert.That(result.AmountAbsorbed, Is.EqualTo(0));
			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(20));
		}

		[Test]
		public void AbsorbDamage_MultipleMatchingShieldsConsumesNewestFirst()
		{
			BattleUnit unit = CreateUnit();
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);

			BattleShieldAbsorptionResult result =
				unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Physical, 12);

			Assert.That(result.AmountAbsorbed, Is.EqualTo(12));
			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(8));
		}

		[Test]
		public void AbsorbDamage_DepletedShieldReturnsBrokenKind()
		{
			BattleUnit unit = CreateUnit();
			unit.BattleAttributes.AddShield(ShieldKind.Magical, 5, durationTurns: -1);

			BattleShieldAbsorptionResult result =
				unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Magical, 10);

			Assert.That(result.AmountAbsorbed, Is.EqualTo(5));
			Assert.That(result.BrokenShieldKinds.Count, Is.EqualTo(1));
			Assert.That(result.BrokenShieldKinds[0], Is.EqualTo(ShieldKind.Magical));
			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		}

		[Test]
		public void AbsorbDamage_SurvivingShieldReturnsNoBrokenKinds()
		{
			BattleUnit unit = CreateUnit();
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

			BattleShieldAbsorptionResult result =
				unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Physical, 5);

			Assert.That(result.BrokenShieldKinds.Count, Is.EqualTo(0));
		}

		[Test]
		public void AdvanceShieldDurations_ExpiresFiniteShieldsAndKeepsInfiniteShields()
		{
			BattleUnit unit = CreateUnit();
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: 1);
			unit.BattleAttributes.AddShield(ShieldKind.Magical, 10, durationTurns: -1);

			unit.BattleAttributes.AdvanceShieldDurations();

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Magical));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(10));
		}

		[Test]
		public void ClearShields_RemovesAllActiveShields()
		{
			BattleUnit unit = CreateUnit();
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 5, durationTurns: -1);
			unit.BattleAttributes.AddShield(ShieldKind.Magical, 5, durationTurns: -1);

			unit.BattleAttributes.ClearShields();

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		}

		[Test]
		public void ApplyShieldEffect_AddsShieldToTargetAndRecordsSourceEvent()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit();

			new ApplyShieldEffect
			{
				Kind = ShieldKind.Magical,
				Amount = 15,
				DurationInTurns = 2
			}.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(target.BattleAttributes.ActiveShields[0].Value.Kind, Is.EqualTo(ShieldKind.Magical));
			Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(15));
			Assert.That(target.BattleAttributes.ActiveShields[0].Value.RemainingTurns, Is.EqualTo(2));
			Assert.That(FindEvent<ApplyShieldRequirement.Event>(target), Is.Null);

			ApplyShieldRequirement.Event shieldEvent = FindEvent<ApplyShieldRequirement.Event>(source);
			Assert.That(shieldEvent, Is.Not.Null);
			Assert.That(shieldEvent.Amount, Is.EqualTo(15));
			Assert.That(shieldEvent.Kind, Is.EqualTo(ShieldKind.Magical));
		}

		[Test]
		public void ApplyShieldEffect_IgnoresNonPositiveAmount()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit();

			new ApplyShieldEffect
			{
				Kind = ShieldKind.Physical,
				Amount = 0,
				DurationInTurns = 1
			}.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
			Assert.That(FindEvent<ApplyShieldRequirement.Event>(source), Is.Null);
		}

		[Test]
		public void DamageTargetEffect_ShieldAbsorbsMatchingDamageBeforeHealth()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(health: 100);
			target.BattleAttributes.AddShield(ShieldKind.Physical, 20, durationTurns: -1);

			CreateDamageEffect(15, MathFormula.DamageInput.Kind.Physical)
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(100));
			Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(5));
		}

		[Test]
		public void DamageTargetEffect_ExcessDamageSpillsIntoHealthAndRecordsEvents()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(health: 100);
			target.BattleAttributes.AddShield(ShieldKind.Physical, 5, durationTurns: -1);

			CreateDamageEffect(15, MathFormula.DamageInput.Kind.Physical)
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(90));
			Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));

			DealDamageRequirement.Event dealEvent = FindEvent<DealDamageRequirement.Event>(source);
			MaxSingleHitDamageRequirement.Event maxHitEvent = FindEvent<MaxSingleHitDamageRequirement.Event>(source);
			TakeDamageRequirement.Event takeEvent = FindEvent<TakeDamageRequirement.Event>(target);
			AbsorbDamageWithShieldRequirement.Event absorbEvent =
				FindEvent<AbsorbDamageWithShieldRequirement.Event>(target);
			ShieldBrokenRequirement.Event brokenEvent = FindEvent<ShieldBrokenRequirement.Event>(target);

			Assert.That(dealEvent, Is.Not.Null);
			Assert.That(dealEvent.Amount, Is.EqualTo(15));
			Assert.That(maxHitEvent, Is.Not.Null);
			Assert.That(maxHitEvent.Amount, Is.EqualTo(15));
			Assert.That(takeEvent, Is.Not.Null);
			Assert.That(takeEvent.Amount, Is.EqualTo(15));
			Assert.That(absorbEvent, Is.Not.Null);
			Assert.That(absorbEvent.Amount, Is.EqualTo(5));
			Assert.That(brokenEvent, Is.Not.Null);
			Assert.That(brokenEvent.Kind, Is.EqualTo(ShieldKind.Physical));
		}

		[Test]
		public void DamageTargetEffect_WrongShieldKindBypassesShield()
		{
			BattleUnit source = CreateUnit();
			BattleUnit target = CreateUnit(health: 100);
			target.BattleAttributes.AddShield(ShieldKind.Magical, 20, durationTurns: -1);

			CreateDamageEffect(15, MathFormula.DamageInput.Kind.Physical)
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(85));
			Assert.That(target.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(target.BattleAttributes.ActiveShields[0].Value.CurrentAmount, Is.EqualTo(20));
			Assert.That(FindEvent<AbsorbDamageWithShieldRequirement.Event>(target), Is.Null);
		}

		[Test]
		public void DamageTargetEffect_VampirismUsesOnlyHealthDamage()
		{
			BattleUnit source = CreateUnit(health: 100, lifeSteal: 1f);
			BattleUnit target = CreateUnit(health: 100);
			source.BattleAttributes.Health.Decrease(50);
			target.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);

			CreateDamageEffect(15, MathFormula.DamageInput.Kind.Physical)
				.Apply(CreateContext(source, target));

			Assert.That(target.BattleAttributes.Health.Current, Is.EqualTo(95));
			Assert.That(source.BattleAttributes.Health.Current, Is.EqualTo(55));
		}

		[Test]
		public void ApplyShieldRequirement_AnyAccumulatesMatchingAmounts()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new ApplyShieldRequirement
				{
					RequiredAmount = 30,
					Filter = ApplyShieldRequirement.KindFilter.Any
				}
			};

			progress.Register(new ApplyShieldRequirement.Event { Amount = 10, Kind = ShieldKind.Physical });
			progress.Register(new ApplyShieldRequirement.Event { Amount = 10, Kind = ShieldKind.Magical });

			Assert.That(progress.CurrentProgress, Is.EqualTo(200f / 3f).Within(0.1f));
		}

		[Test]
		public void ApplyShieldRequirement_FilterIgnoresWrongKind()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new ApplyShieldRequirement
				{
					RequiredAmount = 10,
					Filter = ApplyShieldRequirement.KindFilter.Physical
				}
			};

			progress.Register(new ApplyShieldRequirement.Event { Amount = 10, Kind = ShieldKind.Magical });

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void AbsorbDamageWithShieldRequirement_AccumulatesAcrossHits()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new AbsorbDamageWithShieldRequirement { RequiredAmount = 20 }
			};

			progress.Register(new AbsorbDamageWithShieldRequirement.Event { Amount = 10 });
			progress.Register(new AbsorbDamageWithShieldRequirement.Event { Amount = 10 });

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void MaxDamageAbsorbedInOneHitRequirement_UsesMaximumInsteadOfSum()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new MaxDamageAbsorbedInOneHitRequirement { RequiredAmount = 20 }
			};

			progress.Register(new AbsorbDamageWithShieldRequirement.Event { Amount = 8 });
			progress.Register(new AbsorbDamageWithShieldRequirement.Event { Amount = 8 });
			progress.Register(new AbsorbDamageWithShieldRequirement.Event { Amount = 20 });

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void ShieldBrokenRequirement_CountsMatchingBreaks()
		{
			var progress = new FeatRequirementProgress
			{
				Requirement = new ShieldBrokenRequirement
				{
					RequiredCount = 2,
					Filter = ShieldBrokenRequirement.KindFilter.Physical
				}
			};

			progress.Register(new ShieldBrokenRequirement.Event { Kind = ShieldKind.Magical });
			progress.Register(new ShieldBrokenRequirement.Event { Kind = ShieldKind.Physical });
			progress.Register(new ShieldBrokenRequirement.Event { Kind = ShieldKind.Physical });

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void BattleTurnRules_EndTurnAdvancesShieldDurations()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit unit = fixture.PlayerUnits[0];
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: 2);

			BattleTurnRules.EndTurn(fixture.BattleContext, unit);

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(1));
			Assert.That(unit.BattleAttributes.ActiveShields[0].Value.RemainingTurns, Is.EqualTo(1));
		}

		[Test]
		public void BattleTurnRules_EndTurnRemovesExpiredShield()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit unit = fixture.PlayerUnits[0];
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: 1);

			BattleTurnRules.EndTurn(fixture.BattleContext, unit);

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
		}

		[Test]
		public void BattleContext_ClearRuntimeClearsActiveShields()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
			BattleUnit unit = fixture.PlayerUnits[0];
			unit.BattleAttributes.AddShield(ShieldKind.Physical, 10, durationTurns: -1);
			unit.RecordFeatEvent(new ApplyShieldRequirement.Event { Amount = 10, Kind = ShieldKind.Physical });

			fixture.BattleContext.ClearRuntime();

			Assert.That(unit.BattleAttributes.ActiveShields.Count, Is.EqualTo(0));
			Assert.That(unit.PendingFeatEvents.Count, Is.EqualTo(0));
		}

		[Test]
		public void PlayerVictory_CompletesApplyShieldFeatNode()
		{
			FeatNode applyShieldNode = CreateReachableNode(
				"apply_shield",
				new ApplyShieldRequirement
				{
					RequiredAmount = 15,
					Filter = ApplyShieldRequirement.KindFilter.Physical
				});

			AssertPlayerVictoryCompletesNode(
				applyShieldNode,
				unit => unit.RecordFeatEvent(new ApplyShieldRequirement.Event
				{
					Amount = 15,
					Kind = ShieldKind.Physical
				}));
		}

		[Test]
		public void PlayerVictory_CompletesAbsorbShieldFeatNode()
		{
			FeatNode absorbShieldNode = CreateReachableNode(
				"absorb_shield",
				new AbsorbDamageWithShieldRequirement { RequiredAmount = 20 });

			AssertPlayerVictoryCompletesNode(
				absorbShieldNode,
				unit => unit.RecordFeatEvent(new AbsorbDamageWithShieldRequirement.Event { Amount = 20 }));
		}

		[Test]
		public void PlayerVictory_CompletesShieldBrokenFeatNode()
		{
			FeatNode brokenShieldNode = CreateReachableNode(
				"broken_shield",
				new ShieldBrokenRequirement
				{
					RequiredCount = 1,
					Filter = ShieldBrokenRequirement.KindFilter.Any
				});

			AssertPlayerVictoryCompletesNode(
				brokenShieldNode,
				unit =>
				{
					unit.BattleAttributes.AddShield(ShieldKind.Physical, 5, durationTurns: -1);
					BattleShieldAbsorptionResult result =
						unit.BattleAttributes.AbsorbDamage(MathFormula.DamageInput.Kind.Physical, 10);
					for (int index = 0; index < result.BrokenShieldKinds.Count; index++)
					{
						unit.RecordFeatEvent(new ShieldBrokenRequirement.Event
						{
							Kind = result.BrokenShieldKinds[index]
						});
					}
				});
		}

		private static BattleUnit CreateUnit(int health = 100, float lifeSteal = 0f)
		{
			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes
				{
					Health = health,
					LifeSteal = lifeSteal
				},
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			return new BattleUnit(creatureUnit, BattleSide.Player);
		}

		private static DamageTargetEffect CreateDamageEffect(int baseDamage, MathFormula.DamageInput.Kind kind)
		{
			return new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = baseDamage,
					DamageKind = kind,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};
		}

		private static BattleAbilityExecutionContext CreateContext(BattleUnit source, BattleUnit target)
		{
			return new BattleAbilityExecutionContext
			{
				SourceObject = source,
				TargetObject = target
			};
		}

		private static FeatNode CreateReachableNode(string id, FeatRequirement requirement)
		{
			return new FeatNode
			{
				Id = id,
				DisplayName = id,
				Requirements = new List<FeatRequirement> { requirement },
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward
					{
						Attribute = BonusStatsReward.AttributeType.Health,
						Value = 1
					}
				},
				NeighbourNodeIds = new List<string> { "root" }
			};
		}

		private static void AssertPlayerVictoryCompletesNode(
			FeatNode featNode,
			Action<BattleUnit> recordEvent)
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 2);

			FeatNode rootNode = new FeatNode
			{
				Id = "root",
				DisplayName = "Root",
				NeighbourNodeIds = new List<string> { featNode.Id }
			};

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, featNode },
				RootNodeId = rootNode.Id
			};

			FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			recordEvent(fixture.PlayerUnits[0]);
			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress =
				FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], featNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		private static TEvent FindEvent<TEvent>(BattleUnit unit) where TEvent : FeatRequirement.EventBase
		{
			IReadOnlyList<FeatRequirement.EventBase> events = unit.PendingFeatEvents;
			for (int index = 0; index < events.Count; index++)
			{
				if (events[index] is TEvent typedEvent)
				{
					return typedEvent;
				}
			}

			return null;
		}
	}
}
