using System;
using System.IO;
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
		public void TrySave_WithGameSaveFileData_WritesGameFile()
		{
			IOFileService ioFileService = CreateService();
			GameSaveFileData saveData = CreateSaveFileData();

			bool result = ioFileService.TrySave(saveData);

			Assert.That(result, Is.True);
			Assert.That(File.Exists(ioFileService.SaveFilePath), Is.True);
			Assert.That(File.ReadAllText(ioFileService.SaveFilePath), Does.Contain("\"WorldSeed\": 9876"));
		}

		[Test]
		public void TryLoad_FromSavedGameFile_RestoresSerializedPayload()
		{
			IOFileService ioFileService = CreateService();
			GameSaveFileData saveData = CreateSaveFileData();
			Assert.That(ioFileService.TrySave(saveData), Is.True);

			bool result = ioFileService.TryLoad(out GameSaveFileData loadedSaveData);

			Assert.That(result, Is.True);
			Assert.That(loadedSaveData, Is.Not.Null);
			Assert.That(loadedSaveData.WorldSeed, Is.EqualTo(9876));
			Assert.That(loadedSaveData.RespawnPoint.ToVector3Int(), Is.EqualTo(new Vector3Int(6, 7, 8)));
			Assert.That(loadedSaveData.Player.WorldCell.ToVector3Int(), Is.EqualTo(new Vector3Int(3, 4, 5)));
			Assert.That(loadedSaveData.Player.TeamSlots.Count, Is.EqualTo(GameRule.TeamMemberCount));
			Assert.That(loadedSaveData.Player.TeamSlots[0].HasCreature, Is.True);
			Assert.That(loadedSaveData.Player.TeamSlots[0].Creature.SpeciesResourceId, Is.EqualTo("Creature/CreatureA/CreatureA"));
			Assert.That(loadedSaveData.Player.StoredCreatures.Count, Is.EqualTo(1));
		}

		[Test]
		public void TryLoad_WithoutGameFile_ReturnsFalseAndNullSaveData()
		{
			IOFileService ioFileService = CreateService();

			bool result = ioFileService.TryLoad(out GameSaveFileData loadedSaveData);

			Assert.That(result, Is.False);
			Assert.That(loadedSaveData, Is.Null);
		}

		private IOFileService CreateService()
		{
			return new IOFileService(tempDirectory, "test-save.json");
		}

		private static GameSaveFileData CreateSaveFileData()
		{
			var playerSaveData = new PlayerSaveData
			{
				WorldCell = SerializableVector3Int.From(new Vector3Int(3, 4, 5))
			};

			for (int index = 0; index < GameRule.TeamMemberCount; index++)
			{
				playerSaveData.TeamSlots.Add(new CreatureSlotSaveData());
			}

			playerSaveData.TeamSlots[0] = new CreatureSlotSaveData
			{
				HasCreature = true,
				Creature = new CreatureUnitSaveData
				{
					SpeciesResourceId = "Creature/CreatureA/CreatureA",
					CurrentFormId = "DPS"
				}
			};
			playerSaveData.StoredCreatures.Add(new CreatureUnitSaveData
			{
				SpeciesResourceId = "Creature/CreatureA/CreatureA",
				CurrentFormId = "Default"
			});

			return new GameSaveFileData
			{
				WorldSeed = 9876,
				RespawnPoint = SerializableVector3Int.From(new Vector3Int(6, 7, 8)),
				Player = playerSaveData
			};
		}
	}
}
