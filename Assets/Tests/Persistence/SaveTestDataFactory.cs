using System.Collections.Generic;
using System.Reflection;
using AYellowpaper.SerializedCollections;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Persistence
{
	internal static class SaveTestDataFactory
	{
		public const string SpeciesGuid = "test-species-guid";
		public const string OtherSpeciesGuid = "other-test-species-guid";
		public const string RootNodeId = "root";
		public const string DamageNodeId = "damage_node";
		public const string DefaultFormId = "Default";
		public const string DpsFormId = "DPS";

		public static readonly Vector3 PlayerPosition = new Vector3(3.5f, 4f, 5.5f);
		public static readonly Vector3Int RespawnPoint = new Vector3Int(6, 7, 8);

		private static readonly FieldInfo GuidField = typeof(ReferenceableScriptableObject)
			.GetField("guid", BindingFlags.Instance | BindingFlags.NonPublic);

		public static CreatureSpecies CreateSpecies(string p_guid = SpeciesGuid)
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
			species.name = "Test Species";
			SetGuid(species, p_guid);

			species.Attributes = new Attributes
			{
				Health = 30,
				ActionPoints = 7,
				Movement = 4,
				Attack = 2,
				Armor = 1,
				Magic = 3,
				Resistance = 2,
				Recovery = 5f
			};

			Ability defaultAbility = ScriptableObject.CreateInstance<Ability>();
			defaultAbility.name = "Default Attack";
			species.DefaultAbilities = new List<Ability> { defaultAbility };

			species.Forms = new SerializedDictionary<string, CreatureForm>
			{
				[DefaultFormId] = new CreatureForm { DisplayName = "Default", Tier = 0 },
				[DpsFormId] = new CreatureForm { DisplayName = "DPS", Tier = 1 }
			};

			species.FeatBoard = CreateFeatBoard();
			return species;
		}

		public static FeatBoard CreateFeatBoard()
		{
			FeatNode rootNode = CreateRootNode();
			FeatNode damageNode = CreateDamageNode();

			rootNode.NeighbourNodeIds = new List<string> { damageNode.Id };
			damageNode.NeighbourNodeIds = new List<string> { rootNode.Id };

			return new FeatBoard
			{
				RootNodeId = rootNode.Id,
				Nodes = new List<FeatNode> { rootNode, damageNode }
			};
		}

		public static FeatNode CreateRootNode()
		{
			return new FeatNode
			{
				Id = RootNodeId,
				DisplayName = "Root",
				Requirements = new List<FeatRequirement>(),
				Rewards = new List<FeatReward>()
			};
		}

		public static FeatNode CreateDamageNode()
		{
			Ability unlockedAbility = ScriptableObject.CreateInstance<Ability>();
			unlockedAbility.name = "Unlocked Strike";

			Status unlockedPassive = ScriptableObject.CreateInstance<Status>();
			unlockedPassive.name = "Unlocked Passive";

			return new FeatNode
			{
				Id = DamageNodeId,
				DisplayName = "Damage Node",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement
					{
						RequiredAmount = 100,
						RequiredRepeatCount = 2,
						RequirementScope = FeatRequirement.Scope.Game
					}
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward
					{
						Attribute = BonusStatsReward.AttributeType.Attack,
						Value = 5f
					},
					new AbilityReward
					{
						Ability = unlockedAbility
					},
					new PassiveReward
					{
						Status = unlockedPassive
					}
				}
			};
		}

		public static CreatureUnit CreateCreature(
			CreatureSpecies p_species,
			string p_formId = DpsFormId,
			bool p_includeDamageProgress = false)
		{
			CreatureUnit creature = new CreatureUnit
			{
				Species = p_species,
				CurrentFormID = p_formId,
				FeatBoardProgress = new FeatBoardProgress()
			};

			FeatBoardService.ApplyProgress(creature);

			if (p_includeDamageProgress)
			{
				AddDamageProgress(creature);
			}

			return creature;
		}

		public static FeatNodeProgress AddDamageProgress(
			CreatureUnit p_creature,
			int p_completionCount = 2,
			float p_progress = 40f,
			int p_repeatCount = 1)
		{
			FeatNode node = p_creature?.Species?.FeatBoard?.GetNode(DamageNodeId);
			FeatNodeProgress progress = p_creature?.FeatBoardProgress?.GetOrCreateProgress(node);
			if (progress == null || progress.RequirementProgress.Count == 0)
			{
				return progress;
			}

			progress.CompletionCount = p_completionCount;
			progress.RequirementProgress[0].CurrentProgress = p_progress;
			progress.RequirementProgress[0].CompletedRepeatCount = p_repeatCount;
			return progress;
		}

		public static Ability GetDamageRewardAbility(CreatureSpecies p_species)
		{
			FeatNode node = p_species?.FeatBoard?.GetNode(DamageNodeId);
			if (node?.Rewards == null)
			{
				return null;
			}

			for (int index = 0; index < node.Rewards.Count; index++)
			{
				if (node.Rewards[index] is AbilityReward reward)
				{
					return reward.Ability;
				}
			}

			return null;
		}

		public static Status GetDamageRewardPassive(CreatureSpecies p_species)
		{
			FeatNode node = p_species?.FeatBoard?.GetNode(DamageNodeId);
			if (node?.Rewards == null)
			{
				return null;
			}

			for (int index = 0; index < node.Rewards.Count; index++)
			{
				if (node.Rewards[index] is PassiveReward reward)
				{
					return reward.Status;
				}
			}

			return null;
		}

		public static JObject ExpectedCreatureJson(
			string p_speciesGuid = SpeciesGuid,
			string p_formId = DpsFormId,
			bool p_includeDamageProgress = false,
			int p_damageCompletionCount = 2,
			float p_damageProgress = 40f,
			int p_damageRepeatCount = 1)
		{
			JArray nodes = new JArray
			{
				ExpectedNodeJson(RootNodeId, 1)
			};

			if (p_includeDamageProgress)
			{
				nodes.Add(ExpectedNodeJson(
					DamageNodeId,
					p_damageCompletionCount,
					ExpectedRequirementJson(p_damageProgress, p_damageRepeatCount)));
			}

			return new JObject
			{
				["speciesGuid"] = p_speciesGuid,
				["formId"] = p_formId,
				["featBoard"] = new JObject { ["nodes"] = nodes }
			};
		}

		public static JObject ExpectedNodeJson(
			string p_nodeId,
			int p_completionCount,
			params JObject[] p_requirements)
		{
			JArray requirements = new JArray();
			if (p_requirements != null)
			{
				for (int index = 0; index < p_requirements.Length; index++)
				{
					requirements.Add(p_requirements[index]);
				}
			}

			return new JObject
			{
				["nodeId"] = p_nodeId,
				["completions"] = p_completionCount,
				["requirements"] = requirements
			};
		}

		public static JObject ExpectedRequirementJson(float p_progress, int p_repeats)
		{
			return new JObject
			{
				["progress"] = p_progress,
				["repeats"] = p_repeats
			};
		}

		public static JArray EmptyTeamSlots()
		{
			JArray team = new JArray();
			for (int index = 0; index < GameRule.TeamMemberCount; index++)
			{
				team.Add(new JObject { ["hasCreature"] = false });
			}

			return team;
		}

		public static JObject CreatureSlot(JObject p_creatureJson)
		{
			return new JObject
			{
				["hasCreature"] = true,
				["creature"] = p_creatureJson
			};
		}

		public static JObject ExpectedPlayerJson(
			JArray p_team,
			JArray p_storage,
			Vector3? p_position = null)
		{
			return new JObject
			{
				["position"] = SaveHelper.ToJson(p_position ?? PlayerPosition),
				["team"] = p_team,
				["storage"] = p_storage ?? new JArray()
			};
		}

		public static JObject ExpectedGameSaveJson(
			int p_worldSeed,
			Vector3Int p_respawnPoint,
			JObject p_playerJson)
		{
			return new JObject
			{
				["worldSeed"] = p_worldSeed,
				["respawnPoint"] = SaveHelper.ToJson(p_respawnPoint),
				["player"] = p_playerJson
			};
		}

		public static void AssertJsonEquals(JToken p_expected, JToken p_actual)
		{
			Assert.That(
				JToken.DeepEquals(p_expected, p_actual),
				Is.True,
				$"Expected JSON:\n{p_expected}\nActual JSON:\n{p_actual}");
		}

		private static void SetGuid(ReferenceableScriptableObject p_asset, string p_guid)
		{
			Assert.That(GuidField, Is.Not.Null);
			GuidField.SetValue(p_asset, p_guid);
		}
	}
}
