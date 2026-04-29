using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.AbilityCast
{
	public sealed class AbilityCastRequirementTests
	{
		private readonly List<Object> ownedAssets = new();

		[TearDown]
		public void TearDown()
		{
			for (int index = 0; index < ownedAssets.Count; index++)
			{
				if (ownedAssets[index] != null)
				{
					Object.DestroyImmediate(ownedAssets[index]);
				}
			}

			ownedAssets.Clear();
		}

		[Test]
		public void CastAbilityCountRequirement_SpecificAbilityAccumulatesAcrossEvents()
		{
			Ability ability = CreateAbility("Spark");
			Ability otherAbility = CreateAbility("Guard");
			var progress = new FeatRequirementProgress
			{
				Requirement = new CastAbilityCountRequirement
				{
					Ability = ability,
					RequiredCount = 3
				}
			};

			progress.Register(new CastAbilityCountRequirement.Event { Ability = otherAbility, Count = 1 });
			progress.Register(new CastAbilityCountRequirement.Event { Ability = ability, Count = 1 });
			progress.Register(new CastAbilityCountRequirement.Event { Ability = ability, Count = 1 });
			progress.Register(new CastAbilityCountRequirement.Event { Ability = ability, Count = 1 });

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void CastAbilityCountRequirement_NullAbilityCountsAnyAbility()
		{
			Ability firstAbility = CreateAbility("Spark");
			Ability secondAbility = CreateAbility("Guard");
			var progress = new FeatRequirementProgress
			{
				Requirement = new CastAbilityCountRequirement
				{
					Ability = null,
					RequiredCount = 2
				}
			};

			progress.Register(new CastAbilityCountRequirement.Event { Ability = firstAbility, Count = 1 });
			progress.Register(new CastAbilityCountRequirement.Event { Ability = secondAbility, Count = 1 });

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void CastMultipleAbilitiesInOneTurnRequirement_SpecificAbilityUsesBestTurnCount()
		{
			Ability ability = CreateAbility("Spark");
			Ability otherAbility = CreateAbility("Guard");
			var progress = new FeatRequirementProgress
			{
				Requirement = new CastMultipleAbilitiesInOneTurnRequirement
				{
					Ability = ability,
					RequiredCount = 3
				}
			};

			progress.Register(new CastMultipleAbilitiesInOneTurnRequirement.Event
			{
				Ability = otherAbility,
				AbilityCastCountThisTurn = 3,
				TotalCastCountThisTurn = 3
			});
			progress.Register(new CastMultipleAbilitiesInOneTurnRequirement.Event
			{
				Ability = ability,
				AbilityCastCountThisTurn = 2,
				TotalCastCountThisTurn = 2
			});
			progress.Register(new CastMultipleAbilitiesInOneTurnRequirement.Event
			{
				Ability = ability,
				AbilityCastCountThisTurn = 3,
				TotalCastCountThisTurn = 3
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void CastMultipleAbilitiesInOneTurnRequirement_NullAbilityUsesTotalTurnCount()
		{
			Ability ability = CreateAbility("Spark");
			var progress = new FeatRequirementProgress
			{
				Requirement = new CastMultipleAbilitiesInOneTurnRequirement
				{
					Ability = null,
					RequiredCount = 3
				}
			};

			progress.Register(new CastMultipleAbilitiesInOneTurnRequirement.Event
			{
				Ability = ability,
				AbilityCastCountThisTurn = 1,
				TotalCastCountThisTurn = 3
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
		}

		[Test]
		public void ResolveAbility_RecordsTotalAndInTurnCastEvents()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 100,
				defaultActionPoints: 4);

			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;

			Assert.That(phase.TrySubmitAbility(ability, new[] { enemyCell }), Is.True);
			Assert.That(phase.TrySubmitAbility(ability, new[] { enemyCell }), Is.True);

			BattleUnit caster = fixture.PlayerUnits[0];
			Assert.That(CountEvents<CastAbilityCountRequirement.Event>(caster), Is.EqualTo(2));

			CastMultipleAbilitiesInOneTurnRequirement.Event bestTurnEvent =
				FindBestTurnEvent(caster, ability);
			Assert.That(bestTurnEvent, Is.Not.Null);
			Assert.That(bestTurnEvent.AbilityCastCountThisTurn, Is.EqualTo(2));
			Assert.That(bestTurnEvent.TotalCastCountThisTurn, Is.EqualTo(2));

			orchestrator.Dispose();
		}

		[Test]
		public void EndTurn_RecordsInTurnCastSummaryEvent()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 100,
				defaultActionPoints: 4);

			Ability ability = fixture.CreateDamageAbility(baseDamage: 1, actionPointCost: 1);
			fixture.PlayerSources[0].Abilities.Add(ability);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			PlayerTurnPhase phase = fixture.GetPlayerTurnPhase(orchestrator);
			Vector3Int enemyCell = fixture.EnemyUnits[0].BoardPosition;
			Assert.That(phase.TrySubmitAbility(ability, new[] { enemyCell }), Is.True);
			Assert.That(phase.TrySubmitAbility(ability, new[] { enemyCell }), Is.True);

			Assert.That(phase.TrySubmitEndTurn(), Is.True);

			CastMultipleAbilitiesInOneTurnRequirement.Event summaryEvent =
				FindBestTurnEvent(fixture.PlayerUnits[0], ability);
			Assert.That(summaryEvent, Is.Not.Null);
			Assert.That(summaryEvent.AbilityCastCountThisTurn, Is.EqualTo(2));

			orchestrator.Dispose();
		}

		[Test]
		public void RemoveAbilityReward_RemovesCreatureAbility()
		{
			Ability ability = CreateAbility("Spark");
			var creature = new CreatureUnit
			{
				Abilities = new List<Ability> { ability },
				PermanentPassives = new List<Status>()
			};

			new RemoveAbilityReward { Ability = ability }.Apply(creature);

			CollectionAssert.DoesNotContain(creature.Abilities, ability);
			CollectionAssert.DoesNotContain(creature.GetAbilities(), ability);
		}

		[Test]
		public void RemoveAbilityReward_RemovesDefaultAbilityAfterInitialization()
		{
			Ability ability = CreateAbility("Spark");
			CreatureSpecies species = CreateSpecies("Caster");
			species.DefaultAbilities.Add(ability);

			var creature = new CreatureUnit
			{
				Species = species,
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			creature.AddAbilities(species.DefaultAbilities);

			CollectionAssert.Contains(creature.GetAbilities(), ability);

			new RemoveAbilityReward { Ability = ability }.Apply(creature);

			CollectionAssert.DoesNotContain(creature.GetAbilities(), ability);
		}

		[Test]
		public void AbilityReward_AddsAbilityToCreature()
		{
			Ability ability = CreateAbility("Spark");

			var creature = new CreatureUnit
			{
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			new AbilityReward { Ability = ability }.Apply(creature);

			CollectionAssert.Contains(creature.GetAbilities(), ability);
		}

		[Test]
		public void CompletedUpgradeNode_RemovesOldAbilityAndAddsReplacement()
		{
			Ability oldAbility = CreateAbility("Spark I");
			Ability upgradedAbility = CreateAbility("Spark II");
			CreatureSpecies species = CreateSpecies("Caster");
			species.DefaultAbilities.Add(oldAbility);

			var creature = new CreatureUnit
			{
				Species = species,
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode upgradeNode = new FeatNode
			{
				Id = "upgrade",
				DisplayName = "Spark Upgrade",
				Requirements = new List<FeatRequirement>
				{
					new CastAbilityCountRequirement { Ability = oldAbility, RequiredCount = 2 }
				},
				Rewards = new List<FeatReward>
				{
					new RemoveAbilityReward { Ability = oldAbility },
					new AbilityReward { Ability = upgradedAbility }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(upgradeNode.Id);
			species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, upgradeNode },
				RootNodeId = rootNode.Id
			};

			FeatProgressionService.InitializeCreatureUnit(creature);
			FeatProgressionService.RegisterEvent(
				creature,
				new CastAbilityCountRequirement.Event { Ability = oldAbility, Count = 2 });

			IReadOnlyList<Ability> abilities = creature.GetAbilities();
			CollectionAssert.DoesNotContain(abilities, oldAbility);
			CollectionAssert.Contains(abilities, upgradedAbility);
		}

		private Ability CreateAbility(string name)
		{
			Ability ability = ScriptableObject.CreateInstance<Ability>();
			ability.name = name;
			ownedAssets.Add(ability);
			return ability;
		}

		private CreatureSpecies CreateSpecies(string name)
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
			species.name = name;
			ownedAssets.Add(species);
			return species;
		}

		private static int CountEvents<TEvent>(BattleUnit unit) where TEvent : FeatRequirement.EventBase
		{
			int count = 0;
			for (int index = 0; index < unit.PendingFeatEvents.Count; index++)
			{
				if (unit.PendingFeatEvents[index] is TEvent)
				{
					count++;
				}
			}

			return count;
		}

		private static CastMultipleAbilitiesInOneTurnRequirement.Event FindBestTurnEvent(
			BattleUnit unit,
			Ability ability)
		{
			CastMultipleAbilitiesInOneTurnRequirement.Event bestEvent = null;
			for (int index = 0; index < unit.PendingFeatEvents.Count; index++)
			{
				if (unit.PendingFeatEvents[index] is not CastMultipleAbilitiesInOneTurnRequirement.Event castEvent ||
					castEvent.Ability != ability)
				{
					continue;
				}

				if (bestEvent == null ||
					castEvent.AbilityCastCountThisTurn > bestEvent.AbilityCastCountThisTurn)
				{
					bestEvent = castEvent;
				}
			}

			return bestEvent;
		}
	}
}
