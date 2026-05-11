using Newtonsoft.Json.Linq;
using NUnit.Framework;
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
	}
}
