using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Persistence;
using UnityEngine;

namespace Tests.Services
{
	public sealed class SaveServiceTests
	{
		private ServiceLocatorTestScope serviceLocatorScope;

		[SetUp]
		public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

		[TearDown]
		public void TearDown() => serviceLocatorScope?.Dispose();

		[Test]
		public void Save_WithBoundSaveData_EmitsSaveCompleted()
		{
			SaveService saveService = ServiceLocator.Instance.SaveService;
			GameSaveData saveData = new GameSaveData();
			saveService.BindSaveData(saveData);

			bool saveCompleted = false;
			bool saveSucceeded = false;
			EventCenter.SaveCompleted += OnSaveCompleted;

			try
			{
				bool result = saveService.Save();

				Assert.That(result, Is.True);
				Assert.That(saveCompleted, Is.True);
				Assert.That(saveSucceeded, Is.True);
			}
			finally
			{
				EventCenter.SaveCompleted -= OnSaveCompleted;
			}

			void OnSaveCompleted(GameSaveData data, bool success)
			{
				saveCompleted = true;
				saveSucceeded = success;
			}
		}

		[Test]
		public void CreateSaveJson_UsesWorldSeedAndPlayerServicePayload()
		{
			ServiceLocator.Instance.PlayerService.PlayerData.SetPosition(new Vector3(1.5f, 2f, 3.5f), true);
			ServiceLocator.Instance.WorldService.WorldContext.ApplySeed(42);
			GameSaveData saveData = new GameSaveData();
			saveData.SetRespawnPoint(new Vector3Int(4, 5, 6));
			SaveService saveService = ServiceLocator.Instance.SaveService;
			saveService.BindSaveData(saveData);

			JObject saveJson = saveService.CreateSaveJson();

			Assert.That(saveJson["worldSeed"]?.Value<int>(), Is.EqualTo(42));
			Assert.That(SaveHelper.ToVector3Int(saveJson["respawnPoint"] as JObject), Is.EqualTo(new Vector3Int(4, 5, 6)));
			Assert.That(Vector3Int.FloorToInt(SaveHelper.ToVector3(saveJson["player"]?["position"] as JObject)), Is.EqualTo(new Vector3Int(1, 2, 3)));
		}

		[Test]
		public void TryLoad_WithSaveJson_AppliesWorldSeedAndPlayerData()
		{
			SaveService saveService = ServiceLocator.Instance.SaveService;
			GameSaveData saveData = new GameSaveData();
			saveService.BindSaveData(saveData);

			JObject saveJson = new JObject
			{
				["worldSeed"] = 42,
				["respawnPoint"] = SaveHelper.ToJson(new Vector3Int(4, 5, 6)),
				["player"] = new JObject
				{
					["position"] = SaveHelper.ToJson(new Vector3(1.5f, 2f, 3.5f)),
					["team"] = new JArray(),
					["storage"] = new JArray()
				}
			};

			bool result = saveService.TryLoad(saveJson);

			Assert.That(result, Is.True);
			Assert.That(ServiceLocator.Instance.WorldService.WorldContext.Seed, Is.EqualTo(42));
			Assert.That(ServiceLocator.Instance.PlayerService.PlayerWorldCell, Is.EqualTo(new Vector3Int(1, 2, 3)));
			Assert.That(saveData.WorldSeed, Is.EqualTo(42));
			Assert.That(saveData.RespawnPoint, Is.EqualTo(new Vector3Int(4, 5, 6)));
		}

		[Test]
		public void Save_WithPlayerTeamAndStorage_WritesExpectedJsonAndUpdatesSaveData()
		{
			CreatureSpecies species = SaveTestDataFactory.CreateSpecies();
			ServiceLocator.Instance.ReferenceRegistry.Bind(new[] { species });

			SaveService saveService = ServiceLocator.Instance.SaveService;
			GameSaveData saveData = new GameSaveData();
			saveData.SetRespawnPoint(SaveTestDataFactory.RespawnPoint);
			saveService.BindSaveData(saveData);

			ServiceLocator.Instance.WorldService.WorldContext.ApplySeed(9876);
			PopulateRuntimePlayerData(species);

			bool result = saveService.Save();

			Assert.That(result, Is.True);
			Assert.That(ServiceLocator.Instance.IOFileService.TryLoad(out JObject savedJson), Is.True);
			SaveTestDataFactory.AssertJsonEquals(CreateExpectedGameSaveJson(), savedJson);

			Assert.That(saveData.WorldSeed, Is.EqualTo(9876));
			Assert.That(saveData.RespawnPoint, Is.EqualTo(SaveTestDataFactory.RespawnPoint));
			Assert.That(saveData.Player.Position.Value, Is.EqualTo(SaveTestDataFactory.PlayerPosition));
			Assert.That(saveData.Player.Team[0], Is.Not.Null);
			Assert.That(saveData.Player.Team[0].CurrentFormID, Is.EqualTo(SaveTestDataFactory.DpsFormId));
			Assert.That(saveData.Player.CreatureStorage.Count, Is.EqualTo(1));
		}

		[Test]
		public void TryLoadFromFile_WithExpectedJson_RestoresRuntimeAndSaveData()
		{
			CreatureSpecies species = SaveTestDataFactory.CreateSpecies();
			ServiceLocator.Instance.ReferenceRegistry.Bind(new[] { species });

			SaveService saveService = ServiceLocator.Instance.SaveService;
			GameSaveData saveData = new GameSaveData();
			saveService.BindSaveData(saveData);

			JObject expectedJson = CreateExpectedGameSaveJson();
			Assert.That(ServiceLocator.Instance.IOFileService.TrySave(expectedJson), Is.True);

			bool result = saveService.TryLoadFromFile();

			Assert.That(result, Is.True);
			Assert.That(ServiceLocator.Instance.WorldService.WorldContext.Seed, Is.EqualTo(9876));
			SaveTestDataFactory.AssertJsonEquals(
				expectedJson["player"],
				ServiceLocator.Instance.PlayerService.ToJson(ServiceLocator.Instance.ReferenceRegistry));
			Assert.That(saveData.WorldSeed, Is.EqualTo(9876));
			Assert.That(saveData.RespawnPoint, Is.EqualTo(SaveTestDataFactory.RespawnPoint));
			Assert.That(saveData.Player.Team[0], Is.Not.Null);
			Assert.That(saveData.Player.Team[0].FeatBoardProgress.FindProgress(SaveTestDataFactory.DamageNodeId).CompletionCount, Is.EqualTo(2));
			Assert.That(saveData.Player.CreatureStorage.Count, Is.EqualTo(1));
		}

		private static void PopulateRuntimePlayerData(CreatureSpecies p_species)
		{
			PlayerData playerData = ServiceLocator.Instance.PlayerService.PlayerData;
			playerData.SetPosition(SaveTestDataFactory.PlayerPosition, true);
			playerData.Team[0] = SaveTestDataFactory.CreateCreature(
				p_species,
				SaveTestDataFactory.DpsFormId,
				p_includeDamageProgress: true);
			playerData.Team[4] = SaveTestDataFactory.CreateCreature(
				p_species,
				SaveTestDataFactory.DefaultFormId);
			playerData.CreatureStorage.Add(SaveTestDataFactory.CreateCreature(
				p_species,
				SaveTestDataFactory.DpsFormId));
		}

		private static JObject CreateExpectedGameSaveJson()
		{
			JArray team = SaveTestDataFactory.EmptyTeamSlots();
			team[0] = SaveTestDataFactory.CreatureSlot(
				SaveTestDataFactory.ExpectedCreatureJson(p_includeDamageProgress: true));
			team[4] = SaveTestDataFactory.CreatureSlot(
				SaveTestDataFactory.ExpectedCreatureJson(p_formId: SaveTestDataFactory.DefaultFormId));

			JObject playerJson = SaveTestDataFactory.ExpectedPlayerJson(
				team,
				new JArray { SaveTestDataFactory.ExpectedCreatureJson() });

			return SaveTestDataFactory.ExpectedGameSaveJson(
				9876,
				SaveTestDataFactory.RespawnPoint,
				playerJson);
		}
	}
}
