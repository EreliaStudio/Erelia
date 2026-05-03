using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Feats.FightEventRegistration
{
	public sealed class FightEventRegistrationTests
	{
		[Test]
		public void FightDurationDoesNotCarryPartialProgressAcrossFights()
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
		public void FightDurationCanCompleteWithinOneFight()
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
		public void GameDurationCarriesPartialProgressAcrossFights()
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
		public void RepeatableFightDurationCompletesOncePerQualifyingFight()
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
		public void RepeatableAbilityScopeCompletesForEachQualifyingEvent()
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
