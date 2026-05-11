using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
	public sealed class PlayerServiceTests
	{
		private ServiceLocatorTestScope serviceLocatorScope;

		[SetUp]
		public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

		[TearDown]
		public void TearDown() => serviceLocatorScope?.Dispose();

		[Test]
		public void AddCreatureToTeamOrStorage_ValidCreature_AddsToTeamAndEmitsEvent()
		{
			PlayerService playerService = ServiceLocator.Instance.PlayerService;
			CreatureUnit creature = new CreatureUnit();

			CreatureUnit addedCreature = null;
			EventCenter.PlayerCreatureAdded += OnCreatureAdded;

			try
			{
				bool result = playerService.AddCreatureToTeamOrStorage(creature);

				Assert.That(result, Is.True);
				Assert.That(addedCreature, Is.SameAs(creature));
			}
			finally
			{
				EventCenter.PlayerCreatureAdded -= OnCreatureAdded;
			}

			void OnCreatureAdded(CreatureUnit unit, bool hadOpenSlot) => addedCreature = unit;
		}

		[Test]
		public void ToJson_WithPlayerState_CapturesTeamStorageAndPosition()
		{
			PlayerService playerService = ServiceLocator.Instance.PlayerService;
			CreatureSpecies species = LoadTestSpecies();
			RegisterSpecies(species);

			playerService.PlayerData.SetPosition(new Vector3(3.5f, 4f, 5.5f), true);
			playerService.PlayerData.Team[0] = CreateCreature(species, "DPS");
			playerService.PlayerData.CreatureStorage.Add(CreateCreature(species, "Default"));

			JObject json = playerService.ToJson(ServiceLocator.Instance.ReferenceRegistry);

			Assert.That(Vector3Int.FloorToInt(SaveHelper.ToVector3(json["position"] as JObject)), Is.EqualTo(new Vector3Int(3, 4, 5)));

			JArray team = json["team"] as JArray;
			Assert.That(team, Is.Not.Null);
			Assert.That(team.Count, Is.EqualTo(GameRule.TeamMemberCount));
			Assert.That(team[0]["hasCreature"]?.Value<bool>(), Is.True);
			Assert.That(team[0]["creature"]?["speciesGuid"]?.Value<string>(), Is.EqualTo(species.Guid));
			Assert.That(team[0]["creature"]?["formId"]?.Value<string>(), Is.EqualTo("DPS"));

			JArray storage = json["storage"] as JArray;
			Assert.That(storage, Is.Not.Null);
			Assert.That(storage.Count, Is.EqualTo(1));
			Assert.That(storage[0]["formId"]?.Value<string>(), Is.EqualTo("Default"));
		}

		[Test]
		public void LoadFromJson_WithPlayerJson_RestoresTeamStorageAndPosition()
		{
			PlayerService playerService = ServiceLocator.Instance.PlayerService;
			CreatureSpecies species = LoadTestSpecies();
			RegisterSpecies(species);

			JObject playerJson = CreatePlayerJson(species.Guid);

			bool result = playerService.LoadFromJson(playerJson, ServiceLocator.Instance.ReferenceRegistry);

			Assert.That(result, Is.True);
			Assert.That(playerService.PlayerWorldCell, Is.EqualTo(new Vector3Int(3, 4, 5)));
			Assert.That(playerService.PlayerData.Team[0], Is.Not.Null);
			Assert.That(playerService.PlayerData.Team[0].Species.name, Is.EqualTo("CreatureA"));
			Assert.That(playerService.PlayerData.Team[0].CurrentFormID, Is.EqualTo("DPS"));
			Assert.That(playerService.PlayerData.Team[1], Is.Null);
			Assert.That(playerService.PlayerData.CreatureStorage.Count, Is.EqualTo(1));
			Assert.That(playerService.PlayerData.CreatureStorage.GetAt(0).CurrentFormID, Is.EqualTo("Default"));
		}

		[Test]
		public void PlayerWorldCell_WithPlayerData_ReturnsCurrentWorldCell()
		{
			PlayerService playerService = ServiceLocator.Instance.PlayerService;
			playerService.PlayerData.SetPosition(new Vector3(7.5f, 8f, 9.5f), true);

			Assert.That(playerService.PlayerWorldCell, Is.EqualTo(new Vector3Int(7, 8, 9)));
		}

		private static void RegisterSpecies(CreatureSpecies p_species)
		{
			Assert.That(p_species.Guid, Is.Not.Empty,
				"Species has no GUID. Open the asset in the Unity editor or run Tools/Erelia/Rebuild Reference Database.");
			ServiceLocator.Instance.ReferenceRegistry.Bind(new[] { p_species });
		}

		private static JObject CreatePlayerJson(string p_speciesGuid)
		{
			JArray teamArray = new JArray();
			for (int index = 0; index < GameRule.TeamMemberCount; index++)
			{
				teamArray.Add(new JObject { ["hasCreature"] = false });
			}

			teamArray[0] = new JObject
			{
				["hasCreature"] = true,
				["creature"] = new JObject
				{
					["speciesGuid"] = p_speciesGuid,
					["formId"] = "DPS",
					["featBoard"] = new JObject { ["nodes"] = new JArray() }
				}
			};

			JArray storageArray = new JArray
			{
				new JObject
				{
					["speciesGuid"] = p_speciesGuid,
					["formId"] = "Default",
					["featBoard"] = new JObject { ["nodes"] = new JArray() }
				}
			};

			return new JObject
			{
				["position"] = SaveHelper.ToJson(new Vector3(3.5f, 4f, 5.5f)),
				["team"] = teamArray,
				["storage"] = storageArray
			};
		}

		private static CreatureUnit CreateCreature(CreatureSpecies p_species, string p_formId)
		{
			CreatureUnit creature = new CreatureUnit
			{
				Species = p_species,
				CurrentFormID = p_formId
			};

			FeatBoardService.ApplyProgress(creature);
			return creature;
		}

		private static CreatureSpecies LoadTestSpecies()
		{
			CreatureSpecies species = Resources.Load<CreatureSpecies>("Creature/CreatureA/CreatureA");
			Assert.That(species, Is.Not.Null);
			return species;
		}
	}
}
