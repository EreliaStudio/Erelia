using System;
using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
	public sealed class IOFileServiceTests
	{
		private string tempDirectory;

		[SetUp]
		public void SetUp()
		{
			tempDirectory = Path.Combine(
				Path.GetTempPath(),
				"EreliaSaveFileServiceTests",
				Guid.NewGuid().ToString("N"));
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, true);
			}
		}

		[Test]
		public void TrySave_WithJson_WritesGameFile()
		{
			IOFileService ioFileService = CreateService();
			JObject saveJson = CreateSaveJson();

			bool result = ioFileService.TrySave(saveJson);

			Assert.That(result, Is.True);
			Assert.That(File.Exists(ioFileService.SaveFilePath), Is.True);
			Assert.That(File.ReadAllText(ioFileService.SaveFilePath), Does.Contain("\"worldSeed\": 9876"));
		}

		[Test]
		public void TryLoad_FromSavedGameFile_RestoresSerializedPayload()
		{
			IOFileService ioFileService = CreateService();
			JObject saveJson = CreateSaveJson();
			Assert.That(ioFileService.TrySave(saveJson), Is.True);

			bool result = ioFileService.TryLoad(out JObject loaded);

			Assert.That(result, Is.True);
			Assert.That(loaded, Is.Not.Null);
			Assert.That(loaded["worldSeed"]?.Value<int>(), Is.EqualTo(9876));
			Assert.That(SaveHelper.ToVector3Int(loaded["respawnPoint"] as JObject), Is.EqualTo(new Vector3Int(6, 7, 8)));

			JObject playerJson = loaded["player"] as JObject;
			Assert.That(playerJson, Is.Not.Null);
			Assert.That(Vector3Int.FloorToInt(SaveHelper.ToVector3(playerJson["position"] as JObject)), Is.EqualTo(new Vector3Int(3, 4, 5)));

			JArray teamSlots = playerJson["team"] as JArray;
			Assert.That(teamSlots, Is.Not.Null);
			Assert.That(teamSlots.Count, Is.EqualTo(GameRule.TeamMemberCount));
			Assert.That(teamSlots[0]["hasCreature"]?.Value<bool>(), Is.True);

			JArray storage = playerJson["storage"] as JArray;
			Assert.That(storage, Is.Not.Null);
			Assert.That(storage.Count, Is.EqualTo(1));
		}

		[Test]
		public void TryLoad_WithoutGameFile_ReturnsFalseAndNullJson()
		{
			IOFileService ioFileService = CreateService();

			bool result = ioFileService.TryLoad(out JObject loaded);

			Assert.That(result, Is.False);
			Assert.That(loaded, Is.Null);
		}

		private IOFileService CreateService()
		{
			return new IOFileService(tempDirectory, "test-save.json");
		}

		private static JObject CreateSaveJson()
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
					["speciesGuid"] = "test-species-guid",
					["formId"] = "DPS",
					["featBoard"] = new JObject { ["nodes"] = new JArray() }
				}
			};

			JArray storageArray = new JArray
			{
				new JObject
				{
					["speciesGuid"] = "test-species-guid",
					["formId"] = "Default",
					["featBoard"] = new JObject { ["nodes"] = new JArray() }
				}
			};

			return new JObject
			{
				["worldSeed"] = 9876,
				["respawnPoint"] = SaveHelper.ToJson(new Vector3Int(6, 7, 8)),
				["player"] = new JObject
				{
					["position"] = SaveHelper.ToJson(new Vector3(3.5f, 4f, 5.5f)),
					["team"] = teamArray,
					["storage"] = storageArray
				}
			};
		}
	}
}
